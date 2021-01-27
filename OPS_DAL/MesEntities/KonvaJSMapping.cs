using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesEntities
{
    public class KonvaJSMapping
    {
        public object attrs { get; set; }
        public string className { get; set; }
        public object children { get; set; }
    }

    public class KonvaStage : KonvaJSMapping
    {
        public KonvaStage() {
            className = "Stage";
        }
    }

    public class KonvaLayer
    {
        public string className { get; set; }
        public object attrs { get; set; }
        public object[] children { get; set; }

        public KonvaLayer()
        {
            className = "Layer";
        }
    }

    public class KonvaNode {
        public string id { get; set; }
        public string name { get; set; }
        public string className { get; set; }
        public object attrs { get; set; }
    }

    public class KonvaGroup : KonvaNode {
        public object[] children { get; set; }

        public KonvaGroup()
        {
            className = "Group";
        }
    }
    public class KonvaArrow : KonvaShape
    { 
        public KonvaArrow()
        {
            className = "Arrow";
        }
    }

    public class KonvaShape
    {
        public object attrs { get; set; }
        public string className { get; set; }
    }
     
    public class KonvaText : KonvaShape
    {
        public KonvaText()
        {
            className = "Text";
        }
    }

    public class KonvaLine : KonvaShape
    {
        public KonvaLine()
        {
            className = "Line";
        }
    }
    public class KonvaRect : KonvaShape
    {
        public KonvaRect()
        {
            className = "Rect";
        }
    }

    public class KonvaPosition
    {
        public int AxisX { get; set; }
        public int AxisY { get; set; }
    }

}
