﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using Telerik.Sitefinity.Web;
using Ucommerce.Sitefinity.UI.Api.Model;
using Ucommerce.Sitefinity.UI.Constants;
using UCommerce;
using UCommerce.EntitiesV2;
using UCommerce.Infrastructure;
using UCommerce.Search;
using Product = UCommerce.Documents.Product;

namespace Ucommerce.Sitefinity.UI.Api
{
    public class SearchApiController : ApiController
    {
        private readonly IRepository<UCommerce.EntitiesV2.Product> productRepository;

        public SearchApiController()
        {
            this.productRepository = ObjectFactory.Instance.Resolve<IRepository<UCommerce.EntitiesV2.Product>>();
        }

        [Route(RouteConstants.SEARCH_ROUTE_VALUE)]
        [HttpPost]
        public IHttpActionResult FullText(FullTextModel model)
        {
            var searchResult = UCommerce.Api.SearchLibrary.GetProductsByName(model.SearchQuery);

            return Ok(this.ConvertToFullTextSearchResultModel(searchResult, model.PageId, model.CategoryId));
        }

        [Route(RouteConstants.SEARCH_SUGGESTIONS_ROUTE_VALUE)]
        [HttpPost]
        public IHttpActionResult Suggestions(FullTextModel model)
        {
            var searchResult = UCommerce.Api.SearchLibrary.GetProductNameSuggestions(model.SearchQuery);

            return Ok(searchResult);
        }

        private IList<FullTextSearchResultModel> ConvertToFullTextSearchResultModel(IList<Product> products, Guid pageId, int categoryId)
        {
            var fullTextSearchResultModels = new List<FullTextSearchResultModel>();

            var currency = UCommerce.Runtime.SiteContext.Current.CatalogContext.CurrentPriceGroup.Currency;
            var productsPrices = UCommerce.Api.CatalogLibrary.CalculatePrice(products.Select(x => x.Guid).ToList()).Items;
            ProductCatalog catalog = UCommerce.Api.CatalogLibrary.GetCatalog();

            var productCategory = catalog.Categories
                .Where(c => c.CategoryId == categoryId)
                .FirstOrDefault();
            var culture = System.Threading.Thread.CurrentThread.CurrentUICulture;

            foreach (var product in products)
            {
                string categoryName = null;

                if (productCategory != null)
                {
                    categoryName = productCategory.Name;
                }
                else
                {
                    categoryName = "General";
                }

                var url = string.Concat(GetUrlByPageNodeIdAlternative(pageId, culture, true) + "/" + categoryName + "/" + product.Id);
                var entityProduct = this.productRepository.Get(product.Id);


                var fullTestSearchResultModel = new FullTextSearchResultModel()
                {
                    ThumbnailImageUrl = product.ThumbnailImageUrl,
                    Name = product.Name,
                    Url = url,
                    Price = new Money(productsPrices.First(x => x.ProductGuid == product.Guid).PriceInclTax, currency).ToString(),
                };

                fullTextSearchResultModels.Add(fullTestSearchResultModel);
            }

            return fullTextSearchResultModels;
        }

        private string GetUrlByPageNodeIdAlternative(Guid pageNodeId, CultureInfo targetCulture, bool resolveAsAbsolutUrl)
        {
            var siteMap = SiteMapBase.GetCurrentProvider();

            // Get the siteMapNode
            var siteMapNode = siteMap.FindSiteMapNodeFromKey(pageNodeId.ToString()) as PageSiteNode;


            var url = String.Empty;

            if (siteMapNode != null)
            {
                // Get the URL of the siteMapNode
                url = siteMapNode.GetUrl(targetCulture, false);

                if (resolveAsAbsolutUrl)
                {
                    // Get the absolute URL of the pageNode
                    url = UrlPath.ResolveUrl(url, true, true);
                }
            }

            return url;
        }
    }
}
