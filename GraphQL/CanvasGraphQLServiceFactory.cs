using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using StrawberryShake;
using UvA.DataNose.Connectors.Canvas;
using UvA.DataNose.Connectors.Canvas.Generated;

namespace UvA.Connectors.Canvas.GraphQL
{
    /// <summary>
    /// Factory for creating StrawberryShake-based Canvas GraphQL service instances
    /// </summary>
    public static class CanvasGraphQLServiceFactory
    {
        public static CanvasGraphQLService Create(string baseUrl, string accessToken, string userAgent)
        {
            // Create service collection for dependency injection
            var services = new ServiceCollection();

            // Add HTTP client with authentication
            services.AddHttpClient("CanvasGraphQLClient", client =>
            {
                client.BaseAddress = new Uri($"{baseUrl.TrimEnd('/')}/api/graphql");
                client.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);
                client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", accessToken);
            });

            // Add StrawberryShake GraphQL client
            services
                .AddCanvasGraphQLClient()
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress = new Uri($"{baseUrl.TrimEnd('/')}/api/graphql");
                    client.DefaultRequestHeaders.Authorization = 
                        new AuthenticationHeaderValue("Bearer", accessToken);
                });

            // Build service provider
            var serviceProvider = services.BuildServiceProvider();

            // Get the generated GraphQL client
            var graphQLClient = serviceProvider.GetRequiredService<ICanvasGraphQLClient>();

            return new CanvasGraphQLService(graphQLClient);
        }
    }

    /// <summary>
    /// Service wrapper around the generated StrawberryShake Canvas GraphQL client
    /// </summary>
    public class CanvasGraphQLService(ICanvasGraphQLClient client)
    {
        private readonly ICanvasGraphQLClient _client = client ?? throw new ArgumentNullException(nameof(client));

        /// <summary>
        /// Generic pagination helper for GraphQL queries with cursor-based pagination
        /// </summary>
        /// <typeparam name="TNode">The type of node being paginated</typeparam>
        /// <typeparam name="TPageInfo">The type of page info (must have HasNextPage and EndCursor)</typeparam>
        /// <param name="fetchPage">Function that fetches a single page given a cursor, returns (nodes, pageInfo, errors)</param>
        /// <returns>All collected nodes from all pages</returns>
        private async Task<List<TNode>> PaginateQueryAsync<TNode, TPageInfo>(
            Func<string?, Task<(IEnumerable<TNode?>? nodes, TPageInfo? pageInfo, IReadOnlyList<IClientError>? errors)>> fetchPage)
        {
            var allNodes = new List<TNode>();
            string? cursor = null;
            var hasNextPage = true;

            while (hasNextPage)
            {
                try
                {
                    var (nodes, pageInfo, errors) = await fetchPage(cursor);

                    // Check for GraphQL errors
                    if (errors != null && errors.Any())
                    {
                        var errorMessages = string.Join(", ", errors.Select(e => e.ToString()));
                        throw new Exception($"GraphQL Error: {errorMessages}");
                    }

                    // Extract page info using reflection to get HasNextPage and EndCursor
                    if (pageInfo != null)
                    {
                        var hasNextPageProperty = pageInfo.GetType().GetProperty("HasNextPage");
                        var endCursorProperty = pageInfo.GetType().GetProperty("EndCursor");

                        if (hasNextPageProperty != null && endCursorProperty != null)
                        {
                            hasNextPage = (bool)(hasNextPageProperty.GetValue(pageInfo) ?? false);
                            cursor = endCursorProperty.GetValue(pageInfo) as string;
                        }
                        else
                        {
                            hasNextPage = false;
                        }
                    }
                    else
                    {
                        hasNextPage = false;
                    }

                    // Add nodes if available
                    if (nodes != null)
                    {
                        allNodes.AddRange(nodes.Where(n => n != null).Cast<TNode>());
                    }
                    else
                    {
                        hasNextPage = false;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to fetch: {ex.Message}", ex);
                }
            }

            return allNodes;
        }

        public async Task<List<Submission>> GetAssignmentSubmissions(
            string assignmentId,
            CanvasApiConnector connector,
            SubmissionGradingStatus? gradingStatus = null)
        {
            var allSubmissions = await PaginateQueryAsync<IGetAssignmentSubmission_Assignment_SubmissionsConnection_Nodes, IGetAssignmentSubmission_Assignment_SubmissionsConnection_PageInfo>(
                async cursor =>
                {
                    var result = await _client.GetAssignmentSubmission.ExecuteAsync(
                        assignmentId: assignmentId,
                        gradingStatus: gradingStatus,
                        after: cursor);

                    return (
                        result.Data?.Assignment?.SubmissionsConnection?.Nodes,
                        result.Data?.Assignment?.SubmissionsConnection?.PageInfo,
                        result.Errors
                    );
                }
            );

            return ConvertToSubmissions(allSubmissions, connector);
        }

        private List<Submission> ConvertToSubmissions(
            List<IGetAssignmentSubmission_Assignment_SubmissionsConnection_Nodes> submissions,
            CanvasApiConnector connector)
        {
            return submissions.Select(s => new Submission(connector)
            {
                ID = ExtractIdFromBase64(s.Id),
                UserID = ExtractIdFromBase64(s.UserId).ToString(),
                SubmittedAt = s.SubmittedAt?.DateTime,
                GradedAt = s.GradedAt?.DateTime,
                Score = s.Score,
                Grade = s.Grade ?? string.Empty,
                AssignmentID = int.Parse(s.AssignmentId),
                Connector = connector
            }).ToList();
        }

        /// <summary>
        /// Sets the course post policy using GraphQL mutation
        /// </summary>
        public async Task<bool> SetCoursePostPolicy(string courseId, bool postManually)
        {
            try
            {
                var result = await _client.SetCoursePostPolicy.ExecuteAsync(
                    courseId: courseId,
                    postManually: postManually);

                if (result.Errors.Any())
                {
                    var errorMessages = string.Join(", ", result.Errors.Select(e => e.Message));
                    throw new Exception($"GraphQL Error: {errorMessages}");
                }

                return result.Data?.SetCoursePostPolicy?.PostPolicy?.PostManually == postManually;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to set course post policy: {ex.Message}", ex);
            }
        }


        /// <summary>
        /// Extracts numeric ID from Canvas's base64-encoded GraphQL IDs
        /// </summary>
        private int ExtractIdFromBase64(string base64Id)
        {
            if (string.IsNullOrEmpty(base64Id))
                return 0;

            try
            {
                // First try to parse as a plain integer
                if (int.TryParse(base64Id, out int directId))
                    return directId;

                // Try to decode base64 and extract ID
                byte[] data = Convert.FromBase64String(base64Id);
                string decoded = System.Text.Encoding.UTF8.GetString(data);
                
                // Canvas typically encodes IDs as "Type-123" where 123 is the actual ID
                var parts = decoded.Split('-');
                if (parts.Length >= 2 && int.TryParse(parts.Last(), out int extractedId))
                    return extractedId;

                // If all else fails, try to find numbers in the string
                var numbers = System.Text.RegularExpressions.Regex.Match(decoded, @"\d+");
                if (numbers.Success && int.TryParse(numbers.Value, out int regexId))
                    return regexId;

                System.Diagnostics.Debug.WriteLine($"Could not extract ID from: {base64Id} (decoded: {decoded})");
                return 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error extracting ID from {base64Id}: {ex.Message}");
                return 0;
            }
        }
    }
}
