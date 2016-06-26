// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Anderman.TagHelpers
{
    public class AdditionalValuesMetadataProvider : IDisplayMetadataProvider
    {

        public void GetDisplayMetadata(DisplayMetadataProviderContext context)
        {
            var displayAttribute = context.Attributes.OfType<DisplayAttribute>().FirstOrDefault();
            AddAttribute(context, displayAttribute?.Prompt, nameof(displayAttribute.Prompt));
            AddAttribute(context, displayAttribute?.ShortName, nameof(displayAttribute.ShortName));
        }

        private static void AddAttribute(DisplayMetadataProviderContext context, string value, string keyname)
        {
            if (!string.IsNullOrEmpty(value))
            {
                context.DisplayMetadata.AdditionalValues[keyname] = value;
            }
        }

        /// <summary>
        /// Sets the values for properties of <see cref="P:Microsoft.AspNetCore.Mvc.ModelBinding.Metadata.DisplayMetadataProviderContext.DisplayMetadata"/>. 
        /// </summary>
        /// <param name="context">The <see cref="T:Microsoft.AspNetCore.Mvc.ModelBinding.Metadata.DisplayMetadataProviderContext"/>.</param>
        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            throw new NotImplementedException();
        }
    }
    public static class ModelMetadataExtensions
    {
        /// <summary>
        /// Gets the prompt with given <paramref name="modelMetadata"/>.
        /// </summary>
        /// <param name="modelMetadata"></param><see cref="ModelMetadata"/></param>
        /// <returns></returns>
        public static string GetPlaceholder(this ModelMetadata modelMetadata)
        {
            object value;
            modelMetadata.AdditionalValues.TryGetValue(nameof(DisplayAttribute.Prompt), out value);
            if (value == null) value=modelMetadata.GetDisplayName();
            return value as string;
        }
        public static string GetPrompt(this ModelMetadata modelMetadata)
        {
            object value;
            modelMetadata.AdditionalValues.TryGetValue(nameof(DisplayAttribute.Prompt), out value);
            return value as string;
        }
        public static string GetShortName(this ModelMetadata modelMetadata)
        {
            object value;
            modelMetadata.AdditionalValues.TryGetValue(nameof(DisplayAttribute.ShortName), out value);
            return value as string;
        }
    }
}