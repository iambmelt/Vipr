// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.OData.Client;

namespace Microsoft.OData.ProxyExtensions
{
    public class EntityCollectionImpl<T> : DataServiceCollection<T> where T : EntityBase
    {
        private Func<Tuple<EntityBase, string>> _entity;

        public EntityCollectionImpl()
            : base(null, TrackingMode.None)
        {
        }

        public void SetContainer(Func<Tuple<EntityBase, string>> entity)
        {
            _entity = entity;
        }

        protected override void InsertItem(int index, T item)
        {
            InvokeOnEntity(t =>
            {
                if (t.Item1.Context.GetEntityDescriptor(item) == null)
                    t.Item1.Context.AddRelatedObject(t.Item1, t.Item2, item);
                else
                    t.Item1.Context.AddLink(t.Item1, t.Item2, item);
            });

            base.InsertItem(index, item);
        }

        protected override void ClearItems()
        {
            InvokeOnEntity(t =>
            {
                foreach (var i in this)
                {
                    t.Item1.Context.DeleteLink(t.Item1, t.Item2, i);
                }
            });

            base.ClearItems();
        }

        protected override void RemoveItem(int index)
        {
            InvokeOnEntity(t => t.Item1.Context.DeleteLink(t.Item1, t.Item2, this[index]));

            base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            InvokeOnEntity(t =>
            {
                t.Item1.Context.DeleteLink(t.Item1, t.Item2, this[index]);
                if (t.Item1.Context.GetEntityDescriptor(item) == null)
                    t.Item1.Context.AddRelatedObject(t.Item1, t.Item2, item);
                else
                    t.Item1.Context.AddLink(t.Item1, t.Item2, item);
            });

            base.SetItem(index, item);
        }

        private void InvokeOnEntity(Action<Tuple<EntityBase, string>> action)
        {
            if (_entity != null)
            {
                var tuple = _entity();

                if (tuple.Item1.Context != null && tuple.Item1.Context.GetEntityDescriptor(tuple.Item1) != null)
                {
                    action(tuple);
                }
            }
        }
    }
}
