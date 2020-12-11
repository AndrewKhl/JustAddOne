using JustAddOne.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JustAddOne.Controlls
{
	[Route("[controller]/[action]")]
	public class StorageController : Controller
    {
		public StorageController()
        {

        }

		[HttpGet]
		public void AddNumber(string value)
		{
			ServerModel.Instance.AddValue(value);
		}

		[HttpGet]
		public JsonResult GetTotalNumber()
		{
			return new JsonResult(ServerModel.Instance.Number);
		}
	}
}
