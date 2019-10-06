﻿using Shadowsocks.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Shadowsocks.ViewModel
{
    public class ServerViewModel : ViewModelBase
    {
        public ServerViewModel()
        {
            ServersTreeViewCollection = new ObservableCollection<ServerTreeViewModel>();
        }

        private ObservableCollection<ServerTreeViewModel> _serverTreeViewCollection;
        public ObservableCollection<ServerTreeViewModel> ServersTreeViewCollection
        {
            get => _serverTreeViewCollection;
            set
            {
                if (_serverTreeViewCollection != value)
                {
                    _serverTreeViewCollection = value;
                    OnPropertyChanged();
                    ServersChanged?.Invoke(this, new EventArgs());
                }
            }
        }

        public void ReadServers(List<Server> configs)
        {
            ServersTreeViewCollection.Clear();
            var subTags = new HashSet<string>(configs.Select(server => server.SubTag));
            foreach (var subTag in subTags)
            {
                var sub1 = new ServerTreeViewModel
                {
                    Name = subTag,
                    Type = ServerTreeViewType.Subtag
                };
                var servers = configs.Where(server => server.SubTag == subTag).ToArray();
                var groups = new HashSet<string>(servers.Select(server => server.Group));

                foreach (var group in groups)
                {
                    var sub2 = new ServerTreeViewModel
                    {
                        Name = group,
                        Type = ServerTreeViewType.Group,
                        Parent = sub1
                    };
                    var subServers = servers.Where(server => server.Group == group);
                    foreach (var server in subServers)
                    {
                        server.ServerChanged += ServerChanged;
                        sub2.Nodes.CollectionChanged -= ServerCollection_CollectionChanged;
                        sub2.Nodes.CollectionChanged += ServerCollection_CollectionChanged;
                        sub2.Nodes.Add(new ServerTreeViewModel
                        {
                            Server = server,
                            Type = ServerTreeViewType.Server,
                            Parent = sub2
                        });
                    }
                    sub1.Nodes.CollectionChanged -= ServerCollection_CollectionChanged;
                    sub1.Nodes.CollectionChanged += ServerCollection_CollectionChanged;
                    sub1.Nodes.Add(sub2);
                }

                if (groups.Count == 1)
                {
                    sub1.Nodes[0].Name = subTag;
                    sub1.Nodes[0].Type = ServerTreeViewType.Subtag;
                    sub1.Nodes[0].Parent = null;
                    ServersTreeViewCollection.Add(sub1.Nodes.First());
                }
                else
                {
                    ServersTreeViewCollection.Add(sub1);
                }
            }

            ServersTreeViewCollection.CollectionChanged -= ServerCollection_CollectionChanged;
            ServersTreeViewCollection.CollectionChanged += ServerCollection_CollectionChanged;
        }

        public static IEnumerable<Server> ServerTreeViewModelToList(IEnumerable<ServerTreeViewModel> nodes)
        {
            var res = new List<Server>();
            foreach (var serverTreeViewModel in nodes)
            {
                if (serverTreeViewModel.Type == ServerTreeViewType.Server)
                {
                    res.Add(serverTreeViewModel.Server);
                }
                else
                {
                    res.AddRange(ServerTreeViewModelToList(serverTreeViewModel.Nodes));
                }
            }
            return res;
        }

        private void ServerCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ServersChanged?.Invoke(this, new EventArgs());
        }

        public event EventHandler ServersChanged;

        private void ServerChanged(object sender, EventArgs e)
        {
            ServersChanged?.Invoke(this, new EventArgs());
        }
    }
}
