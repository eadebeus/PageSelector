using System;
using System.Collections.Generic;


namespace PageSelectorDemo
{
    public interface ISourceFile
    {
        int NumPages { get; set; }
        bool OpenFile(string pdfFile);
        void CloseFile();
        PageSize GetPageSize(int iPage);
        List<string> GetBookmarksForPage(int iPageToFind);
    }

    public class PageSize
    {
        public float Width;
        public float Height;
    }
}
