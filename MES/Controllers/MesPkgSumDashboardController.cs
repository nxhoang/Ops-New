using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Wordprocessing;
using MES.Models;
using MES.Repositories;
using Newtonsoft.Json;
using OPS_DAL.DAL;
using OPS_DAL.MesBus;
using OPS_DAL.MesEntities;
using OPS_Utils;
using PdfRpt.Core.Contracts;
namespace MES.Controllers
{
    [SessionTimeout]
    public class MesPkgSumDashboardController : Controller
    {
        public string BaseURL = String.Empty;
        public MesPkgSumDashboardController()
        {
        }
        // GET: MesPkgSumDashboard
        public ActionResult Index()
        {
            return View();
        }
        public string LoadProcessSummary(string MXPackage, string ViewType, string SortingPara)
        {
            BaseURL = $"http://{HttpContext.Request.Url.Host}:{HttpContext.Request.Url.Port}/";
            //Get Data of MX_OPDT Summary
            var data = MXOpdtBus.GetMXPkdOPSum(MXPackage);
            var OrigiData = data.ToArray(); //2020-09-12 Tai Le(Thomas)
            int trackingOPSerial = 0;
            Dictionary<string, KonvaPosition> konvaShapePositions = new Dictionary<string, KonvaPosition>();
            int OPGroupCounter = 0,
                Counter = 0,
                LineCounter = 0;
            int CanvasWidth = 1366, CanvasHeight = 768;
            int MaxOPperLine = 0;
            string DisplayColor = "";
            try
            {
                if (data.Count > 0)
                {
                    //2020-10-01 Tai Le(Thomas): Sort source data
                    switch (SortingPara)
                    {
                        case "OPSERIAL":
                            data = data.OrderBy(p => p.OPGROUP).ThenBy(p => p.OPSERIAL).ToList();
                            break;
                        case "OPNUM":
                            data = data.OrderBy(p => p.OPGROUP).ThenBy(p => p.OPNUM).ToList();
                            break;
                    }
                    //::END     2020-10-01 Tai Le(Thomas): Sort source data
                    /* konva global Setting */
                    //Global Gap
                    int GapWidth = 10,
                        GapHeight = 10;
                    int GlobalTitleAxisX = 0,
                        GlobalTitleAxisY = 0,
                        GlobalTitleWidth = 1000,
                        GlobalTitleHeight = 0;// 50;
                    //Global
                    int GlobalBeginLineAxisX = 0 + GapWidth,
                        GlobalBeginLineAxisY = GlobalTitleHeight + GapHeight + GapHeight;
                    //OPDT
                    int OPDTAxisX = 0,
                        OPDTAxisY = 0,
                        OPDTWidth = 60,
                        OPDTHeight = 80;
                    int OPGHeight = 15; //2020-10-01 Tai Le(Thomas) 
                    int txGNameY = 4; //2020-10-01 Tai Le(Thomas) 
                    int txOPNameY = OPGHeight + 2; //2020-10-01 Tai Le(Thomas) 
                    int txNumberCountY = txOPNameY + 10; //2020-10-01 Tai Le(Thomas) 
                    //Konva Layer
                    KonvaLayer konvaLayer = new KonvaLayer();
                    konvaLayer.attrs = new { };
                    List<object> konvaLayerChild = new List<object>();
                    //Dictionary<string, KonvaPosition> konvaShapePositions = new Dictionary<string, KonvaPosition>();
                    //Get very early OP_Serial which has 1st Next_OP such as the 1st OP of each OP_Group
                    //var IndependentOPs = data.Where(p => !data.Any(p2 => p.OPSERIAL.ToString() == p2.NEXTOP));
                    //Draw TITLE:
                    //KonvaGroup konvaGroupTitle = new KonvaGroup();
                    //konvaGroupTitle.attrs = new { x = 0, y = 0, rotation = 0 };
                    //konvaGroupTitle.children = new KonvaShape[] {
                    //    new KonvaText (){
                    //        attrs = new {
                    //            width = GlobalTitleWidth,
                    //            height = GlobalTitleHeight,
                    //            text = $"{data[0].MXPACKAGE}\n\n\t\t\t- Target Qty: {data[0].MXTARGET}\n\n\t\t\t- Style Code: {data[0].STYLECODE}, Style Size:{data[0].STYLESIZE}, Style Color Serial:{data[0].STYLECOLORSERIAL}, RevNo:{data[0].REVNO}",
                    //            fontFamily = "Calibri",
                    //            fontSize = 9,
                    //            fontStyle = "bold" ,
                    //            x = GlobalTitleAxisX,
                    //            y = GlobalTitleAxisY,
                    //            align = "left"
                    //        }
                    //    }
                    //};
                    //konvaLayerChild.Add(konvaGroupTitle);
                    List<object> konvaGroupChild = new List<object>();
                    bool IsDrawNextOP = false;
                    string nextOP = "";
                    string PrevOPGroup = "";
                    //2020-09-12 Tai Le(Thomas)
                    List<MXOpdt> IndependentOPs = new List<MXOpdt>();
                    if (ViewType == "Gant")
                    {
                        var DataHasNextOP = OrigiData.Where(p => !String.IsNullOrEmpty(p.NEXTOP)).ToList();
                        //Get the OPDT having Next OP , but it's not child of any OPSerial
                        IndependentOPs = data.Where(p => !DataHasNextOP.Any(p2 => (p.OPSERIAL.ToString() == p2.NEXTOP))).OrderBy(p => p.OPGROUP).ThenBy(p => p.OPSERIAL).ThenBy(p => p.NEXTOP).ToList();
                        DataHasNextOP.Clear();
                    }
                    else
                    {
                        IndependentOPs = OrigiData.ToList();
                    }
                    foreach (var item in IndependentOPs)
                    {
                        Counter += 1;
                        var runningOPDT = item;
                    HE_nextOP:
                        if (konvaGroupChild.Count > 0)
                            konvaGroupChild.Clear();
                        if (ViewType == "Gant")
                            if (!String.IsNullOrEmpty(nextOP))
                            {
                                var nextOPDTobj = data.Where(p => p.OPSERIAL.ToString() == nextOP).FirstOrDefault();
                                if (nextOPDTobj != null)
                                {
                                    runningOPDT = nextOPDTobj;
                                    //if (runningOPDT.OPGROUP != PrevOPGroup)
                                    //{
                                    //    //LineCounter += 1;
                                    //    //OPGroupCounter = 0; 
                                    //    runningOPDT = null;
                                    //    nextOP = "";
                                    //    goto HE_Cont;
                                    //}
                                }
                                else
                                {
                                    nextOP = "";
                                    goto HE_Cont;
                                }
                            }
                        //Next Position(X,Y) to start drawing OPDT
                        OPDTAxisX = GlobalBeginLineAxisX + (OPDTWidth + GapWidth) * OPGroupCounter;
                        OPDTAxisY = GlobalBeginLineAxisY + (OPDTHeight + GapHeight) * LineCounter;
                        trackingOPSerial = runningOPDT.OPSERIAL;
                        if (konvaShapePositions.ContainsKey(runningOPDT.OPSERIAL.ToString()))
                        {
                            nextOP = "";
                            continue;
                        }
                        //::START  OPDT Canvas
                        KonvaGroup konvaGroupOPDT = new KonvaGroup();
                        //konvaGroupOPDT.attrs = new { x = OPDTAxisX, y = OPDTAxisY, rotation = 0, zIndex = 2 };
                        konvaGroupOPDT.attrs = new { x = OPDTAxisX, y = OPDTAxisY, rotation = 0 };
                        konvaGroupOPDT.id = runningOPDT.OPSERIAL.ToString();
                        konvaGroupOPDT.name = runningOPDT.OPNAME;
                        //Konva Rect => Main
                        var konvaRectOPDTRect = new KonvaRect();
                        konvaRectOPDTRect.attrs = new
                        {
                            x = 0,
                            y = 0,
                            width = OPDTWidth,
                            height = OPDTHeight,
                            fill = "whitesmoke",
                            opacity = runningOPDT.IOTCounter >= 1 ? 1 : 0.85,
                            //stroke = Math.Max(runningOPDT.OPDGSCOMPQTY, runningOPDT.OPIOTCOMPQTY) > runningOPDT.MXTARGET ? "green" : "black",
                            //strokeWidth = 0.75,
                            //stroke = "black",
                            strokeWidth = 0,
                            cornerRadius = new int[] { 5, 5, 0, 0 },
                            shadowBlur = 2,
                            perfectDrawEnabled = false
                        };
                        konvaGroupChild.Add(konvaRectOPDTRect);
                        //2020-10-01 Tai Le(Thomas): Konva Rect for OP Group Name
                        DisplayColor = runningOPDT.DISPLAYCOLOR;
                        if (String.IsNullOrEmpty(DisplayColor))
                        {
                            DisplayColor = "lightblue";
                        }
                        else
                        {
                            if (DisplayColor.Length < 7)
                            {
                                for (int i = 1; i <= (7 - DisplayColor.Length); i++)
                                    DisplayColor += "0";
                            }
                        }
                        konvaGroupChild.Add(new KonvaRect()
                        {
                            attrs = new
                            {
                                x = 0,
                                y = 0,
                                width = OPDTWidth,
                                height = OPGHeight,
                                strokeWidth = 0,
                                cornerRadius = new int[] { 5, 5, 0, 0 },
                                fillLinearGradientStartPoint = new { x = 0, y = 0 },
                                fillLinearGradientEndPoint = new { x = 0, y = OPGHeight },
                                fillLinearGradientColorStops = new object[] { 0, DisplayColor, 1, "white" },
                                perfectDrawEnabled = false
                            }
                        });
                        //2020-10-01 Tai Le(Thomas): Konva Line to seperate GroupName and Detail
                        konvaGroupChild.Add(new KonvaLine()
                        {
                            attrs = new
                            {
                                points = new int[] { 0, OPGHeight, OPDTWidth, OPGHeight },
                                strokeWidth = 0.25,
                                stroke = "black"
                            }
                        });
                        //Konva Text: OP Group Name
                        var konvaTextOPGText = new KonvaText();
                        konvaTextOPGText.attrs = new
                        {
                            width = OPDTWidth * 0.95,
                            //text = $"({OPDTAxisX},{OPDTAxisY}) {runningOPDT.OPGROUPNAME.ToUpper()}",
                            text = $"{runningOPDT.OPGROUPNAME.ToUpper()}",
                            fill = "black",
                            fontSize = 5,
                            fontStyle = "bold",
                            x = 1,
                            y = txGNameY,
                            align = "center",
                            wrap = "none"
                        };
                        konvaGroupChild.Add(konvaTextOPGText);
                        //Konva Text: OP Process Name
                        //Handle OPName
                        var _opName = runningOPDT.OPNAME.Length > 27 ? String.Format("{0}...", runningOPDT.OPNAME.Substring(0, 27)) : runningOPDT.OPNAME;
                        var konvaTextOPDTText = new KonvaText();
                        konvaTextOPDTText.attrs = new
                        {
                            width = OPDTWidth * 0.95,
                            text = $"{runningOPDT.OPSERIAL}: {_opName}",
                            //text = $"{runningOPDT.OPSERIAL}, next: {runningOPDT.NEXTOP}",
                            fill = "black",
                            fontSize = 8,
                            fontStyle = "normal",
                            x = 1,
                            y = txOPNameY,
                            align = "center",
                            wrap = "none"
                        };
                        konvaGroupChild.Add(konvaTextOPDTText);
                        //Konva Text: Archieve Qty
                        var konvaTextOPDTTextArcQty = new KonvaText();
                        konvaTextOPDTTextArcQty.attrs = new
                        {
                            width = OPDTWidth * 0.95 / 2,
                            text = $"Finish\n{Math.Max(runningOPDT.OPDGSCOMPQTY, runningOPDT.OPIOTCOMPQTY)}",
                            fill = "green",
                            fontSize = 10,
                            //fontStyle = Math.Max(runningOPDT.OPDGSCOMPQTY, runningOPDT.OPIOTCOMPQTY) > runningOPDT.MXTARGET ? "bold" : "normal",
                            //fontStyle = Math.Max(runningOPDT.OPDGSCOMPQTY, runningOPDT.OPIOTCOMPQTY) > 0 ? "bold" : "normal",
                            x = 1,
                            y = txNumberCountY,
                            align = "right"
                        };
                        konvaGroupChild.Add(konvaTextOPDTTextArcQty);
                        //Konva Text: Target Qty
                        var konvaTextOPDTTextTarQty = new KonvaText();
                        konvaTextOPDTTextTarQty.attrs = new
                        {
                            width = OPDTWidth * 0.95 / 2,
                            text = $"Target\n{runningOPDT.MXTARGET}",
                            fill = "blue",
                            fontSize = 10,
                            //fontStyle = Math.Max(runningOPDT.OPDGSCOMPQTY, runningOPDT.OPIOTCOMPQTY) > runningOPDT.MXTARGET ? "bold" : "normal",
                            //fontStyle = Math.Max(runningOPDT.OPDGSCOMPQTY, runningOPDT.OPIOTCOMPQTY) > 0 ? "bold" : "normal",
                            x = 1 + OPDTWidth / 2,
                            y = txNumberCountY,
                            align = "right"
                        };
                        konvaGroupChild.Add(konvaTextOPDTTextTarQty);
                        //Konva Image: OP Employee Image
                        var EmpImages = GetEmpImage(runningOPDT.MXPACKAGE, runningOPDT.OPSERIAL);
                        if (EmpImages.Count > 0)
                        {
                            foreach (var _item in EmpImages)
                            {
                                var konvaGroupImage = new KonvaGroup();
                                konvaGroupImage.attrs = new { x = 1, y = OPDTHeight/1.7, rotation = 0 };
                                konvaGroupImage.children = new object[] {
                                    new {
                                        className="Image" ,
                                        attrs= new {
                                                    x=0 ,
                                                    y=0 ,
                                                    width=OPDTWidth/2.1-3,
                                                    height=OPDTHeight/2.3-3,
                                                    stroke = 0,
                                                    id=_item.EMPLOYEECODE,
                                                    corp= _item.EMPCORP,
                                                    //urlImg = String.Concat("http://hrmvn.pungkookvn.com", _item.EMPIMGPATH.Replace("~",""))
                                                    //urlImg = _item.EMPIMGPATH 
                                                    urlImg= $"{BaseURL}HRMAssets/{_item.EMPCORP}/{_item.ImageName}"
                                                }
                                    }
                                };
                                konvaGroupChild.Add(konvaGroupImage);
                            }
                        }
                        //::END  OPDT Canvas  
                        konvaGroupOPDT.children = konvaGroupChild.ToArray();
                        konvaLayerChild.Add(konvaGroupOPDT);
                        //System.Diagnostics.Debug.WriteLine($"X={OPDTAxisX}; Y={OPDTAxisY}");
                        //Save each Rect Position {X, Y}
                        konvaShapePositions.Add(runningOPDT.OPSERIAL.ToString(), new KonvaPosition() { AxisX = OPDTAxisX, AxisY = OPDTAxisY });
                        PrevOPGroup = runningOPDT.OPGROUP;
                    HE_Cont:
                        //Decide Next OPDT  Positon (X,Y)
                        if (ViewType != "Gant")
                        {
                            if (runningOPDT.OPGROUP != runningOPDT.NEXTOPGROUP)
                            {
                                LineCounter += 1;
                                MaxOPperLine = Math.Max(MaxOPperLine, OPGroupCounter);
                                OPGroupCounter = 0;
                            }
                            else
                            {
                                OPGroupCounter += 1;
                            }
                        }
                        else
                        {
                            //viewType===     Gant
                            //Main Comparision is NextOP 
                            //if (runningOPDT.OPGROUP != runningOPDT.NEXTOPGROUP)
                            //{
                            //    LineCounter += 1;
                            //    OPGroupCounter = 0;
                            //}
                            if (runningOPDT != null)
                            {
                                if (!String.IsNullOrEmpty(runningOPDT.NEXTOP))
                                {
                                    OPGroupCounter += 1;
                                }
                                else if (String.IsNullOrEmpty(runningOPDT.NEXTOP))
                                {
                                    LineCounter += 1;
                                    MaxOPperLine = Math.Max(MaxOPperLine, OPGroupCounter);
                                    OPGroupCounter = 0;
                                }
                            }
                        }
                        if (ViewType == "Gant")
                        {
                            if (runningOPDT != null)
                            {
                                if (!String.IsNullOrEmpty(runningOPDT.NEXTOP))
                                {
                                    nextOP = runningOPDT.NEXTOP;
                                    IsDrawNextOP = true;
                                }
                                else
                                {
                                    nextOP = "";
                                    IsDrawNextOP = false;
                                }
                                if (IsDrawNextOP)
                                    goto HE_nextOP;
                            }
                        }
                    }
                    //Handle the rest OPDT
                    //Handle Konva Arrow  
                    if (ViewType == "WArrow" || ViewType == "Gant")
                    {
                        var OPHasNext = data
                            .Where(p => !String.IsNullOrEmpty(p.NEXTOP))
                            .OrderBy(o1 => o1.OPGROUP)
                            .ThenBy(o2 => o2.OPSERIAL)
                            .ToList();
                        if (OPHasNext.Count > 0)
                        {
                            foreach (var item in OPHasNext)
                            {
                                if (konvaShapePositions.ContainsKey(item.OPSERIAL.ToString()) && konvaShapePositions.ContainsKey(item.NEXTOP))
                                {
                                    var StartPoint = konvaShapePositions[item.OPSERIAL.ToString()];
                                    var EndPoint = konvaShapePositions[item.NEXTOP];
                                    KonvaArrow konvaTextOPDTArrow = new KonvaArrow();
                                    konvaTextOPDTArrow.attrs = new
                                    {
                                        points = konvaArrowPoints(StartPoint, EndPoint, OPDTWidth, OPDTHeight),
                                        pointerLength = 1, //Head 
                                        pointerWidth = 1, //Head
                                        fill = "black", //Head
                                        stroke = "black", //Tale
                                        strokeWidth = 2,//Tale
                                        name = $"{item.OPSERIAL}-{item.NEXTOP}"
                                        //,zIndex = 1
                                    };
                                    konvaLayerChild.Add(konvaTextOPDTArrow);
                                }
                            }
                        }
                    }
                    //::END   Handle Konva Arrow  
                    konvaLayer.children = konvaLayerChild.ToArray();
                    //Konva Stage
                    KonvaStage konvaStage = new KonvaStage();
                    konvaStage.attrs = new { width = CanvasWidth, height = CanvasHeight };
                    //konvaStage.className = "Stage";
                    List<object> konvaStageChild = new List<object>();
                    konvaStageChild.Add(konvaLayer);
                    konvaStage.children = konvaStageChild.ToArray();
#if debug 
    //StringBuilder sb = new StringBuilder();
    //foreach (var item in data)
    //{
    //    if (!konvaShapePositions.ContainsKey(item.OPSERIAL.ToString()))
    //        sb.AppendLine(item.OPSERIAL.ToString());
    //} 
#endif
                    return JsonConvert.SerializeObject(new { konvaStage, MaxOPperLine = (MaxOPperLine + 1) * 1.03 * (OPDTWidth + GapWidth) });
                }
                return JsonConvert.SerializeObject(new
                {
                    konvaStage = new KonvaStage()
                    {
                        attrs = new { width = CanvasWidth, height = CanvasHeight },
                        children = new object[] {
                            new KonvaLayer(){
                                attrs= new { },
                                children = new object[] {
                                    new KonvaText() {
                                        attrs= new { width=888 , height=100, text="No MES execution plan found for this package", fontSize=14, fontStyle="bold", x=20,y=20}
                                    }
                                }
                            }
                        }
                    }
                    ,MaxOPperLine = 0
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject($"Error at OPSerial: {trackingOPSerial}, Counter: {Counter}, {ex.Message}");
            }
        }
        private List<MXOpdt> GetEmpImage(string pMXPackage, int pOPSerial)
        {
            List<MXOpdt> EmpImages = new List<MXOpdt>();
            string query =
                $@"
select t_mx_opdt_mc.OPSERIAL , t_mx_opdt_mc.EMPID , t_mx_opdt_mc.IOT_MODULE_MAC
from t_mx_opdt_mc
join t_mx_opdt on 
    t_mx_opdt_mc.STYLECODE  = t_mx_opdt.STYLECODE and 
    t_mx_opdt_mc.STYLESIZE  = t_mx_opdt.STYLESIZE and 
    t_mx_opdt_mc.STYLECOLORSERIAL  = t_mx_opdt.STYLECOLORSERIAL and 
    t_mx_opdt_mc.REVNO  = t_mx_opdt.REVNO and 
    t_mx_opdt_mc.OPREVNO  = t_mx_opdt.OPREVNO and 
    t_mx_opdt_mc.OPSERIAL  = t_mx_opdt.OPSERIAL  
join t_mx_opmt  on 
    t_mx_opmt.STYLECODE  = t_mx_opdt.STYLECODE and 
    t_mx_opmt.STYLESIZE  = t_mx_opdt.STYLESIZE and 
    t_mx_opmt.STYLECOLORSERIAL  = t_mx_opdt.STYLECOLORSERIAL and 
    t_mx_opmt.REVNO  = t_mx_opdt.REVNO and 
    t_mx_opmt.OPREVNO  = t_mx_opdt.OPREVNO
where 1=1 
and t_mx_opmt.MXPACKAGE = '{pMXPackage}'
and t_mx_opdt_mc.OPSERIAL = {pOPSerial}
";
            var opdtmcEmp = MySqlDBManager.GetObjectsConvertType<OpdtMc>(query, System.Data.CommandType.Text, null);
            string combindedString = String.Join("','", opdtmcEmp.Select(p => p.EMPID));
            //query =
            //    $" SELECT EMPID , PATH , CORP " +
            //    $" FROM T_HR_EMP_MASTER@HRMVNDB A " +
            //    $" WHERE A.EMPIDOLD IN ('{combindedString}') " +
            //    $"      OR A.EMPID IN ('{combindedString}') " +
            //    $"      OR A.SYS_EMPID IN ('{combindedString}') ";
            //var dt = OracleDbManager.Query(query, null, OPS_Utils.ConstantGeneric.ConnectionStr);
            query =
                $" SELECT EmployeeCode as EMPID , ImageUrl as PATH , CorporationCode as CORP ,  ImageName " +
                $" FROM t_hr_empm " +
                $" WHERE EmployeeCode IN ('{combindedString}') ";
            var dt = MySqlDBManager.QueryToDatable(mySQLConnString: OPS_Utils.ConstantGeneric.ConnectionStrMesMySql,
                                                   commandText: query,
                                                   commandType: CommandType.Text,
                                                   parameters: null);
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        var EMPIMGPATH = "";
                        if (item[1] != null)
                            EMPIMGPATH = item[1].ToString();
                        var EMPCORP = "";
                        if (item[2] != null)
                            EMPCORP = item[2].ToString();
                        var ImageName = "";
                        if (item[3] != null)
                            ImageName = item[3].ToString();
                        EmpImages.Add(new MXOpdt()
                        {
                            EMPLOYEECODE = $"{pOPSerial}{item[0]}",
                            EMPCORP = EMPCORP,
                            EMPIMGPATH = EMPIMGPATH,
                            ImageName = ImageName
                        });
                    }
                }
                dt.Dispose();
            }
            return EmpImages;
        }
        public int[] konvaArrowPoints(KonvaPosition StartPoint, KonvaPosition EndPoint, int ShapeWidth, int ShapeHeight)
        {
            //Decide where destination location
            int adjustment = 0; // 20;
            if (EndPoint.AxisY == StartPoint.AxisY)
            {
                //Same line
                if (EndPoint.AxisX > StartPoint.AxisX)
                {
                    //Right 
                    return new int[] { adjustment + StartPoint.AxisX + ShapeWidth, adjustment + StartPoint.AxisY + ShapeHeight / 2, adjustment + EndPoint.AxisX, adjustment + EndPoint.AxisY + ShapeHeight / 2 };
                }
                else //if (EndPoint.AxisX < StartPoint.AxisX)
                {
                    //Left
                    return new int[] { adjustment + StartPoint.AxisX, StartPoint.AxisY + ShapeHeight / 2, adjustment + EndPoint.AxisX + ShapeWidth, adjustment + EndPoint.AxisY + ShapeHeight / 2 };
                }
            }
            else if (EndPoint.AxisY > StartPoint.AxisY)
            {
                //Below 
                if (EndPoint.AxisX > StartPoint.AxisX)
                {
                    //Right
                    return new int[] { adjustment + StartPoint.AxisX + ShapeWidth, adjustment + StartPoint.AxisY + ShapeHeight / 2, adjustment + EndPoint.AxisX + ShapeWidth / 2, adjustment + EndPoint.AxisY };
                }
                else if (EndPoint.AxisX < StartPoint.AxisX)
                {
                    //Left
                    return new int[] { adjustment + StartPoint.AxisX, adjustment + StartPoint.AxisY + ShapeHeight / 2, adjustment + EndPoint.AxisX + ShapeWidth / 2, adjustment + EndPoint.AxisY };
                }
                else //if (EndPoint.AxisX == StartPoint.AxisX)
                {
                    //Under
                    return new int[] { adjustment + StartPoint.AxisX + ShapeWidth / 2, adjustment + StartPoint.AxisY + ShapeHeight, adjustment + EndPoint.AxisX + ShapeWidth / 2, adjustment + EndPoint.AxisY };
                }
            }
            else if (EndPoint.AxisY < StartPoint.AxisY)
            {
                //Above
                if (EndPoint.AxisX > StartPoint.AxisX)
                {
                    //Right
                    return new int[] { adjustment + StartPoint.AxisX + ShapeWidth / 2, adjustment + StartPoint.AxisY, adjustment + EndPoint.AxisX + ShapeWidth / 2, adjustment + EndPoint.AxisY + ShapeHeight };
                }
                else if (EndPoint.AxisX < StartPoint.AxisX)
                {
                    //Left
                    return new int[] { adjustment + StartPoint.AxisX + ShapeWidth / 2, adjustment + StartPoint.AxisY, adjustment + EndPoint.AxisX + ShapeWidth / 2, adjustment + EndPoint.AxisY + ShapeHeight };
                }
                else //if (EndPoint.AxisX == StartPoint.AxisX)
                {
                    //Above
                    return new int[] { adjustment + StartPoint.AxisX + ShapeWidth / 2, adjustment + StartPoint.AxisY, adjustment + EndPoint.AxisX + ShapeWidth / 2, adjustment + EndPoint.AxisY + ShapeHeight };
                }
            }
            else
                return new int[] { 0, 0, 0, 0 };
        }
        public string GetMESPackagesByFactoryDate(GridSettings gridRequest)
        {
            string factory = Url.RequestContext.HttpContext.Request["factory"];
            string date = Url.RequestContext.HttpContext.Request["date"];
            try
            {
                string strSQL = "";
                strSQL =
                    $@"
SELECT ROW_NUMBER() OVER(ORDER BY  t_mx_mpdt.MXPACKAGE ) AS RANKING ,
t_mx_mpdt.MXPACKAGE  , t_mx_mpmt.STYLECODE  , t_mx_mpmt.STYLESIZE  , t_mx_mpmt.STYLECOLORSERIAL  , t_mx_mpmt.REVNO , t_mx_mpdt.MXTARGET , t_mx_mpdt.PLNSTARTDATE , 
t_00_stmt.BUYERSTYLENAME ,  t_00_scmt.STYLECOLORWAYS , t_mx_mpmt.MESFACTORY , v_mesgroup_ao.AONO , t_cm_line.LINENAME 
FROM t_mx_mpmt   
JOIN t_mx_mpdt ON 
    t_mx_mpmt.PACKAGEGROUP = t_mx_mpdt.PACKAGEGROUP
JOIN t_cm_line ON 
    t_mx_mpdt.LINESERIAL = t_cm_line.LINESERIAL 
LEFT JOIN t_00_stmt ON 
    t_mx_mpmt.STYLECODE  = t_00_stmt.STYLECODE 
LEFT JOIN t_00_scmt ON 
    t_mx_mpmt.STYLECODE  = t_00_scmt.STYLECODE 
    AND t_mx_mpmt.STYLECOLORSERIAL = t_00_scmt.STYLECOLORSERIAL
LEFT JOIN v_mesgroup_ao ON 
    t_mx_mpdt.PACKAGEGROUP = v_mesgroup_ao.PACKAGEGROUP 
";
                var _Result = GridData.GetGridDataMySQL(
                    ConstantGeneric.ConnectionStrMesMySql,
                    strSQL,
                    $@" t_mx_mpmt.MESFACTORY = '{factory}' AND DATE_FORMAT(t_mx_mpdt.PLNSTARTDATE, ""%Y%m%d"") like '%{date}%' ",
                    gridRequest,
                    "dd MMM, yyyy");
                return _Result;
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { retResult = false, dataRow = ex.Message });
            }
        }
    }
}