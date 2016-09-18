using System;
using System.Collections.Generic;
using System.IO;


namespace PageSelectorDemo
{
    public class MockSourceFile : ISourceFile
    {
        // Simulate TestCase.pdf
        public int NumPages { get { return 22; } set { } }
        public bool OpenFile(string pdfFile) { 
            return pdfFile != null && Path.GetFileName(pdfFile).CompareTo("TestCase.pdf") == 0; 
        }
        public void CloseFile() { }
        public PageSize GetPageSize(int iPage)
        {
            if(iPage == 9)
                return new PageSize() { Width = 17f * 72f, Height = 11f * 72f };
            else
                return new PageSize() { Width = 8.5f * 72f, Height = 11f * 72f };
        }
        public List<string> GetBookmarksForPage(int iPageToFind)
        {
            switch(iPageToFind) {
                case 4:
                    return new List<string>() { "<<  /Staple 0 /OutputType (Stacker) >> setpagedevice" };
                case 8:
                    return new List<string>() {
                        "<<  /Staple 0 /OutputType (Stacker) >> setpagedevice",
                        "<<  /Jog 0 >> setpagedevice",
                        "<<  /MediaType (cover) /MediaType (cover) /MediaWeight 210.0 /MediaFrontCoating (Glossy) /MediaBackCoating (Glossy) >> setpagedevice"
                    };
                case 9:
                    return new List<string>() { "<<  /MediaType (bond) /MediaType (bond) /MediaWeight 90.0 /MediaFrontCoating (Any) /MediaBackCoating (Any) >> setpagedevice" };
                case 14:
                    return new List<string>() {
                        "<<  /Jog 3 >> setpagedevice",
                        "<<  /Jog 0 >> setpagedevice",
                        "<<  /MediaType (cover) /MediaType (cover) /MediaWeight 210.0 /MediaFrontCoating (Glossy) /MediaBackCoating (Glossy) >> setpagedevice"
                    };
                case 15:
                    return new List<string>() { "<<  /MediaType (bond) /MediaType (bond) /MediaWeight 90.0 /MediaFrontCoating (Any) /MediaBackCoating (Any) >> setpagedevice" };
                case 22:
                    return new List<string>() { "<<  /Jog 3 >> setpagedevice" };
                default:
                    return null;
            }
        }
        public MockSourceFile()
        {
        }
    }
}
