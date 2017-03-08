using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SoftfairTEST.Controllers {
	public class HomeController : Controller {
		public ActionResult Index() {


			var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SoftfairTEST.SLS_NES2800P_a.pdf");
			//PDF auspacken
			byte[] BT4allFile = null;
			iTextSharp.text.pdf.PdfReader reader;
			try {
				reader = new iTextSharp.text.pdf.PdfReader(stream);
				iTextSharp.text.pdf.PdfDictionary root = reader.Catalog;
				iTextSharp.text.pdf.PdfDictionary names = root.GetAsDict(iTextSharp.text.pdf.PdfName.NAMES);
				if (names != null) {
					iTextSharp.text.pdf.PdfDictionary embeddedFiles = names.GetAsDict(iTextSharp.text.pdf.PdfName.EMBEDDEDFILES);
					if (embeddedFiles != null) {

						var en = embeddedFiles.Keys.GetEnumerator();
						while (en.MoveNext()) {
							var obj = embeddedFiles.GetAsArray(en.Current as iTextSharp.text.pdf.PdfName);

							iTextSharp.text.pdf.PdfDictionary fileSpec = obj.GetAsDict(1);

							iTextSharp.text.pdf.PdfDictionary file = fileSpec.GetAsDict(iTextSharp.text.pdf.PdfName.EF);

							foreach (iTextSharp.text.pdf.PdfName key in file.Keys) {
								iTextSharp.text.pdf.PRStream innerstream = (iTextSharp.text.pdf.PRStream)
									iTextSharp.text.pdf.PdfReader.GetPdfObject(file.GetAsIndirectObject(key));

								if (innerstream != null) {
									BT4allFile = iTextSharp.text.pdf.PdfReader.GetStreamBytes(innerstream);
									break;
								}
							}
						}
					}
				}
			}catch(Exception ex) {

			}

			//Request mit Post des Falls und Response mit der ID
			string StartID = null;
			if (BT4allFile != null) {
				try {
					var Req = System.Net.WebRequest.Create("http://localhost/BT4All/SV/d.svc/m?i=getStartID_SLS_NoSign");
					Req.Method = "POST";
					Req.ContentLength = BT4allFile.Length;
					Req.GetRequestStream().Write(BT4allFile, 0, BT4allFile.Length);
					var resp = Req.GetResponse();
					Newtonsoft.Json.JsonTextReader jReader = new Newtonsoft.Json.JsonTextReader(
						new System.IO.StreamReader(resp.GetResponseStream()));
					while (jReader.Read()) {
						var tp = jReader.TokenType;
						var val = jReader.Value;
						if(tp == Newtonsoft.Json.JsonToken.PropertyName && (val as string) == "sid") {
							StartID = jReader.ReadAsString();
							break;
						}
					}

				}catch(Exception ex) {

				}
			}
			//=>iframe wird in der Views/Home/INDEX.cshtml aufgebaut
			this.ViewBag.StartID = StartID;


			return View();
		}

		public ActionResult About() {
			ViewBag.Message = "Your application description page.";

			return View();
		}

		public ActionResult Contact() {
			ViewBag.Message = "Your contact page.";

			return View();
		}
	}
}