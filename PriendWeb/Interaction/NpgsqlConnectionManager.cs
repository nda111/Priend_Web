using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PriendWeb.Interaction
{
    /// <summary>
    /// A class that manages database connection in multi-thread application
    /// </summary>
    public class NpgsqlConnectionManager : IDisposable
    {
        private bool isDisposed = false;
        private object locker = new object();
        private LinkedList<NpgsqlConnection> connectionQueue = new LinkedList<NpgsqlConnection>();
        private List<int> hashCodes = new List<int>();

        /// <summary>
        /// Weather this manager has connection left
        /// </summary>
        public bool HasConnection => connectionQueue.Count != 0;

        /// <summary>
        /// Create and open some connections
        /// </summary>
        /// <param name="connectionString">The connection string of the database connection</param>
        /// <param name="connectionCount">The number of connections</param>
        /// <param name="concurrent">Indicate weather establish each connections in each threads</param>
        public NpgsqlConnectionManager(string connectionString, int connectionCount, bool concurrent)
        {
            lock (locker)
            {
                if (concurrent)
                {
                    NpgsqlConnection[] connections = new NpgsqlConnection[connectionCount];

                    var result = Parallel.For(0, connectionCount, i =>
                    {
                        NpgsqlConnection conn = new NpgsqlConnection(connectionString);
                        conn.Open();

                        connections[i] = conn;
                    });

                    while (!result.IsCompleted) ;

                    foreach (var conn in connections)
                    {
                        hashCodes.Add(conn.GetHashCode());
                        connectionQueue.AddLast(conn);
                    }
                }
                else
                {
                    while (connectionCount-- > 0)
                    {
                        NpgsqlConnection conn = new NpgsqlConnection(connectionString);
                        conn.Open();

                        hashCodes.Add(conn.GetHashCode());
                        connectionQueue.AddLast(conn);
                    }
                }
            }
        }

        /// <summary>
        /// Get the database connection
        /// </summary>
        /// <returns>Get database connection if left, null otherwise</returns>
        public NpgsqlConnection GetConnectionOrNull()
        {
            if (isDisposed)
            {
                return null;
            }

            lock (locker)
            {
                if (HasConnection)
                {
                    var connNode = connectionQueue.First;
                    connectionQueue.RemoveFirst();

                    return connNode.Value;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Wait for there's any database connection
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The dataabse connection</returns>
        public NpgsqlConnection WaitForConnection(CancellationToken cancellationToken = default)
        {
            var conn = GetConnectionOrNull();
            for (; conn == null; conn = GetConnectionOrNull())
            {
                if (isDisposed)
                {
                    return null;
                }
                else if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }
            }

            return conn;
        }

        /// <summary>
        /// Return the database connectoin got
        /// </summary>
        /// <param name="conn">The database connection</param>
        /// <returns>True if the connection belongs to this instance, false otherwises</returns>
        public bool TryReturnConnection(NpgsqlConnection conn)
        {
            int index = hashCodes.BinarySearch(conn.GetHashCode());

            if (index >= 0)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    if (isDisposed)
                    {
                        conn.Dispose();
                        hashCodes.RemoveAt(index);
                    }
                    else
                    {
                        connectionQueue.AddLast(conn);
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public void Dispose()
        {
            isDisposed = true;

            while (connectionQueue.Count != 0)
            {
                var conn = connectionQueue.First.Value;
                conn.Dispose();

                connectionQueue.RemoveFirst();
                hashCodes.Remove(conn.GetHashCode());
            }
        }
    }
}
