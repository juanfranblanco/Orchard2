using Orchard.ContentManagement.Records;
using Orchard.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Orchard.ContentManagement.Data {
    public interface IContentItemStore : IDependency {
        void Store(ContentItem contentItem);
        ContentItem Get(int id);
        ContentItem Get(int id, VersionOptions options);
        IReadOnlyList<ContentItem> GetMany(VersionOptions options, Func<ContentItemVersionRecord, bool> query);
        IReadOnlyList<ContentItem> GetMany(IEnumerable<int> ids);
    }
}