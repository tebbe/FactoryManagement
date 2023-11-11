﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Routing;

namespace FactoryManagement.Infrastructure.Helpers
{
    public static class HtmlHelpers
    {
        /// <summary>
        /// Returns a checkbox for each of the provided <paramref name="items"/>.
        /// </summary>
        public static MvcHtmlString CheckBoxListFor<TModel, TValue>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TValue>> expression, List<SelectListItem> items, object htmlAttributes = null)
        {
            var listName = ExpressionHelper.GetExpressionText(expression);

            return htmlHelper.CheckBoxList(listName, items, htmlAttributes);
        }


        /// <summary>
        /// Stolen from Ben: https://github.com/benfoster/Fabrik.Common/blob/master/src/Fabrik.Common.Web/HtmlHelperExtensions.cs
        /// Returns a checkbox for each of the provided <paramref name="items"/>.
        /// </summary>
        public static MvcHtmlString CheckBoxList(this HtmlHelper htmlHelper, string listName, List<SelectListItem> items, object htmlAttributes = null)
        {
            var container = new TagBuilder("ul");
            container.MergeAttributes(new RouteValueDictionary(htmlAttributes), true);
            foreach (var item in items)
            {
                var li = new TagBuilder("li");
                var label = new TagBuilder("label");

                var cb = new TagBuilder("input");
                cb.MergeAttribute("type", "checkbox");
                cb.MergeAttribute("name", listName);
                cb.MergeAttribute("value", item.Value ?? item.Text);
                if (item.Selected)
                {
                    cb.MergeAttribute("checked", "checked");
                }

                label.InnerHtml = cb.ToString(TagRenderMode.SelfClosing) + item.Text;
                li.InnerHtml = label.ToString();

                container.InnerHtml += li.ToString();
            }

            return new MvcHtmlString(container.ToString());
        }
    }
}