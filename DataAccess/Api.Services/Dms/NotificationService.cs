using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.Domain;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityRequests.Masters;
using Api.Domain.EntityResponses.Dms;
using Api.Extensions;
using Api.Repository.Masters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using static System.Environment;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing;
//using SixLabors.ImageSharp.Drawing.Processing; // For drawing extensions
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.Drawing.Processing;
using System.Data;
using System.Reflection;
using Path = System.IO.Path;
using Api.Domain.EntityResponses;
using Newtonsoft.Json.Serialization;
using Api.Domain.Formatters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Api.Domain.Enum;
using NetTopologySuite.Index.HPRtree;
using Api.DataAccess.Models.Systems;
using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Api.Repository.Dms;
using Org.BouncyCastle.Bcpg;

namespace Api.Services.Masters
{
	public class NotificationService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, IConfiguration configuration, INotificationRepository repository) 
		: BaseService(accessor, languageRepository, configuration)

	{

		public async Task<object> NotificationList(RequestNotification request)
		{
			await ValidateInputRequestAsync(request);
			var result = await repository.GetListAsync(request.DocumentTitle, request.DTFrom, request.DTTo, request.Page, request.Limit);
			var TotalRecord = result.TotalRecord;
			return new ResponsePagination
			{
				TotalRecords = TotalRecord,
				TotalPages = GetTotalPages(TotalRecord, request.Limit),
				Data = result.Records
			};
		}

		//public async Task<object> AdvSearchDocument(RequestAdvSearch request)
		//{
		//	await ValidateInputRequestAsync(request);
		//	var result = await repository.SearchListDocument(request.DocumentTitle, request.Page, request.Limit);
		//	var TotalRecord = result.TotalRecord;
		//	return new ResponsePagination
		//	{
		//		TotalRecords = TotalRecord,
		//		TotalPages = GetTotalPages(TotalRecord, request.Limit),
		//		Data = result.Records
		//	};
		//}

	}
}
