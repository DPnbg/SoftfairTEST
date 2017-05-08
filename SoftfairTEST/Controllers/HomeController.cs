using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SoftfairTEST.Controllers {
	public class HomeController : Controller {
		public ActionResult Index() {



			return View();
		}

        public ActionResult OpenNBG(string fname)
        {
			string err = null;
			string rawHtml = null;
			string StartID = null;


			if (string.IsNullOrWhiteSpace(fname)) {
				err = "Der übergebene Parameter fname darf n icht leer sein. Starten Sie am besten die Anwendung über die Index.cshtml.";
			} else {
				var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(fname);
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
				} catch (Exception ex) {
					err = ex.ToString();
				}

				//Request mit Post des Falls und Response mit der ID
				if (BT4allFile != null) {
					try {
						var Req = System.Net.WebRequest.Create("https://test.nuernberger-bt4all.de/BT4All/SV/d.svc/m?i=getStartID_SLS_NoSign");
						Req.Method = "POST";

						var xtraData = new byte[100];
						for (int x = 0; x < xtraData.Length; x++)
							xtraData[x] = 1;

						Req.ContentLength = BT4allFile.Length + xtraData.Length;
						if (xtraData.Length > 0) {
							Req.Headers.Add("xtraDataLen", xtraData.Length.ToString());
						}
						var reqS = Req.GetRequestStream();
						reqS.Write(BT4allFile, 0, BT4allFile.Length);
						reqS.Write(xtraData, 0, xtraData.Length);
						var resp = Req.GetResponse();
						var respStream = resp.GetResponseStream();
						var respData = new byte[resp.ContentLength];
						respStream.Read(respData, 0,respData.Length);

						try {
							using (var mem = new System.IO.MemoryStream(respData)) {
								Newtonsoft.Json.JsonTextReader jReader = new Newtonsoft.Json.JsonTextReader(
									new System.IO.StreamReader(mem));
								while (jReader.Read()) {
									var tp = jReader.TokenType;
									var val = jReader.Value;
									if (tp == Newtonsoft.Json.JsonToken.PropertyName && (val as string) == "sid") {
										StartID = jReader.ReadAsString();
										break;
									}
								}
							}
						} catch {
							using (var strReader = new System.IO.StreamReader(new System.IO.MemoryStream(respData))) {
								err = "Fehler beim Parsen des JSON Objekts. Serverresponse: ";
								rawHtml = strReader.ReadToEnd();
							}
						}


					} catch (Exception ex) {
						err = ex.ToString();
					}
				}
			}
			//=>iframe wird in der Views/Home/INDEX.cshtml aufgebaut
			ViewBag.err = err;
			ViewBag.rawHtml = rawHtml;
            ViewBag.StartID = StartID;
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