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
        /// <param name="fetchPage">Function that fetches a single page given a cursor, returns (nodes, pageInfo, errors)</param>
        /// <returns>All collected nodes from all pages</returns>
        private async Task<List<TNode>> PaginateQuery<TNode>(
            Func<string?, Task<(IEnumerable<TNode?> Nodes,
                bool HasNextPage,
                string? EndCursor,
                IReadOnlyList<IClientError>? Errors)>> fetchPage)
        {
            var allNodes = new List<TNode>();
            string? cursor = null;

            while (true)
            {
                var (nodes, hasNextPage, endCursor, errors) = await fetchPage(cursor);

                // Handle GraphQL errors
                if (errors is { Count: > 0 })
                {
                    var message = string.Join(", ", errors.Select(e => e.Message));
                    throw new Exception($"GraphQL Error: {message}");
                }

                // Collect nodes
                if (nodes != null)
                {
                    // Filter nulls defensively (StrawberryShake types can be nullable)
                    allNodes.AddRange(nodes.Where(n => n is not null));
                }

                // Stop if no next page or no usable cursor
                if (!hasNextPage || string.IsNullOrWhiteSpace(endCursor))
                    break;

                cursor = endCursor;
            }

            return allNodes;
        }

        public async Task<List<Submission>> GetAssignmentSubmissions(
            string assignmentId,
            CanvasApiConnector connector,
            SubmissionGradingStatus? gradingStatus = null)
        {
            var allNodes = await PaginateQuery(async cursor =>
            {
                var result = await _client.GetAssignmentSubmission.ExecuteAsync(
                    assignmentId: assignmentId,
                    gradingStatus: gradingStatus,
                    after: cursor);

                var connection = result.Data?.Assignment?.SubmissionsConnection;
                var pageInfo = connection?.PageInfo;

                return (
                    Nodes: connection?.Nodes ?? Enumerable.Empty<IGetAssignmentSubmission_Assignment_SubmissionsConnection_Nodes>(),
                    HasNextPage: pageInfo?.HasNextPage ?? false,
                    pageInfo?.EndCursor,
                    result.Errors
                );
            });

            return ConvertToSubmissions(allNodes, connector);
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
