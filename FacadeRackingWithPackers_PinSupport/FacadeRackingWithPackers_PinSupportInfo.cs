using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace FacadeRackingWithPackers_PinSupport
{
    public class FacadeRackingWithPackers_PinSupportInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "FacadeRackingWithPackersPinSupport";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("e3e94f10-2651-45c2-806d-b6bb5551f85f");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
