using Microsoft.Extensions.Logging;
using Orchard.ContentManagement.Handlers;
using Orchard.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Orchard.ContentManagement {
    public interface IContentExporter : IDependency {
        XElement Export(ContentItem contentItem);
    }

    public class DefaultContentExporter : IContentExporter {
        private readonly IEnumerable<IContentHandler> _handlers;
        private readonly IContentManager _contentManager;
        private readonly ILogger _logger;


        private const string Published = "Published";
        private const string Draft = "Draft";

        public DefaultContentExporter(
            IEnumerable<IContentHandler> handlers,
            IContentManager contentManager,
            ILoggerFactory loggerFactory) {
            _handlers = handlers;
            _contentManager = contentManager;
            _logger = loggerFactory.CreateLogger<DefaultContentExporter>();
        }

        public XElement Export(ContentItem contentItem) {
            var context = new ExportContentContext(contentItem, new XElement(XmlConvert.EncodeLocalName(contentItem.ContentType)));

            _handlers.Invoke(contentHandler => contentHandler.Exporting(context), _logger);
            _handlers.Invoke(contentHandler => contentHandler.Exported(context), _logger);

            if (context.Exclude) {
                return null;
            }

            context.Data.SetAttributeValue("Id", _contentManager.GetItemMetadata(contentItem).Identity.ToString());
            if (contentItem.IsPublished()) {
                context.Data.SetAttributeValue("Status", Published);
            }
            else {
                context.Data.SetAttributeValue("Status", Draft);
            }

            return context.Data;
        }
    }
}
