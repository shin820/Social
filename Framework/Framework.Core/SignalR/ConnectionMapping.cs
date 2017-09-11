using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Core.SignalR
{
    public class ConnectionMapping<T>
    {
        private readonly Dictionary<T, HashSet<string>> _connections =
           new Dictionary<T, HashSet<string>>();

        public int Count
        {
            get
            {
                return _connections.Count;
            }
        }

        public void Add(T key, string connectionId)
        {
            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(key, out connections))
                {
                    connections = new HashSet<string>();
                    _connections.Add(key, connections);
                }

                lock (connections)
                {
                    connections.Add(connectionId);
                }
            }
        }

        public void Add(T key, HashSet<string> connectionIds)
        {
            if (!connectionIds.Any())
            {
                return;
            }

            lock (_connections)
            {
                if (!_connections.ContainsKey(key))
                {
                    _connections.Add(key, connectionIds);
                }
            }
        }

        public HashSet<string> GetConnections(Func<T, bool> keyFunc)
        {
            var connections = _connections
                .Where(t => keyFunc(t.Key))
                .SelectMany(t => t.Value)
                .Distinct();

            return new HashSet<string>(connections);
        }

        public IEnumerable<string> GetConnections(T key)
        {
            HashSet<string> connections;
            if (_connections.TryGetValue(key, out connections))
            {
                return connections;
            }

            return Enumerable.Empty<string>();
        }

        public void Remove(T key)
        {
            lock (_connections)
            {
                if (_connections.ContainsKey(key))
                {
                    _connections.Remove(key);
                }
            }
        }

        public void Remove(T key, string connectionId)
        {
            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(key, out connections))
                {
                    return;
                }

                lock (connections)
                {
                    connections.Remove(connectionId);

                    if (connections.Count == 0)
                    {
                        _connections.Remove(key);
                    }
                }
            }
        }

        public void Remove(string connectionId)
        {
            lock (_connections)
            {
                foreach (var connections in _connections.Values)
                {
                    connections.Remove(connectionId);
                }

                var emptyKeys = _connections.Where(t => t.Value.Count == 0).Select(t => t.Key).ToImmutableList();
                foreach (var emptyKey in emptyKeys)
                {
                    _connections.Remove(emptyKey);
                }
            }
        }

        public IImmutableList<T> GetKeys()
        {
            return _connections.Keys.ToImmutableList();
        }

        public IImmutableList<T> GetKeys(IEnumerable<string> connctions)
        {
            return _connections.Where(t => t.Value.Intersect(connctions).Any()).Select(t => t.Key).ToImmutableList();
        }

        public void RefreshKey(T key)
        {
            lock (_connections)
            {
                HashSet<string> connections;
                bool hasConnections = _connections.TryGetValue(key, out connections);
                _connections.Remove(key);
                if (hasConnections)
                {
                    _connections.Add(key, connections);
                }
            }
        }

        public bool IsExist(T key)
        {
            lock (_connections)
            {
                return _connections.ContainsKey(key);
            }
        }
    }
}
