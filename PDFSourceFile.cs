using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using iTextSharp.text;
using iTextSharp.text.pdf;


namespace PageSelectorDemo
{
    public class PDFSourceFile : ISourceFile
    {
        public PDFSourceFile()
        {
            NumPages = 0;
            Reader = null;
            Bookmarks = null;
        }

        public PdfReader Reader { get; set; }

        public int NumPages { get; set; }
        private ArrayList Bookmarks { get; set; }

        public bool OpenFile(string pdfFile)
        {
            try {
                FileInfo fileInfo = new FileInfo(pdfFile);
                if(fileInfo.Length >= 20000000L)
                    Reader = new PdfReader(new RandomAccessFileOrArray(pdfFile), null);
                else
                    Reader = new PdfReader(pdfFile);
                if(Reader == null)
                    return false;
                NumPages = Reader.NumberOfPages;
                Bookmarks = GetBookmarks();
            }
            catch(Exception) {
                return false;
            }
            return true;
        }

        public void CloseFile()
        {
            if(Reader != null)
                Reader.Close();
            Reader = null;
        }

        public PageSize GetPageSize(int iPage)
        {
            Rectangle rect = Reader.GetCropBox(iPage);
            return new PageSize() { Width = rect.Width, Height = rect.Height };
        }

        private ArrayList GetBookmarks()
        {
            Reader.ConsolidateNamedDestinations();
            return SimpleBookmark.GetBookmark(Reader);
        }

        public List<string> GetBookmarksForPage(int iPageToFind)
        {
            return GetBookmarksForPage(Bookmarks, iPageToFind);
        }

        private List<string> GetBookmarksForPage(ArrayList bookmarks, int iPageToFind)
        {
            List<string> list = new List<string>();
            if(bookmarks == null)
                return null;
            try {
                foreach(Hashtable bookmark in bookmarks) {
                    int iPage;
                    if((iPage = GetPageNumber(bookmark)) > 0) {
                        if(iPage == iPageToFind)
                            list.Add(bookmark.ContainsKey("Title") ? (string)bookmark["Title"] : "");
                        else if(bookmark.ContainsKey("Kids")) {
                            ArrayList kids = bookmark["Kids"] as ArrayList;
                            if(kids != null) {
                                List<string> bookmarkStrings = GetBookmarksForPage(kids, iPageToFind);
                                if(bookmarkStrings != null)
                                    list.AddRange(bookmarkStrings);
                            }
                        }
                    }
                }
            }
            catch(Exception) {
                return null;
            }
            return list.Count > 0 ? list : null;
        }

        private static int GetPageNumber(Hashtable bookmark)
        {
            int iPage = -1;
            try {
                if(bookmark.ContainsKey("Action") && "GoTo".Equals(bookmark["Action"])) {
                    string sPage = bookmark.ContainsKey("Page") ? (string)bookmark["Page"] : null;
                    if(sPage != null) {
                        sPage = sPage.Trim();
                        int iSpace = sPage.IndexOf(' ');
                        if(iSpace < 0)
                            iPage = (int)Int64.Parse(sPage);
                        else
                            iPage = (int)Int64.Parse(sPage.Substring(0, iSpace));
                        if(iPage < 0)
                            iPage = -iPage;
                    }
                }
            }
            catch(Exception) {
                iPage = -1;
            }
            return iPage;
        }

        // Methods for debugging/automated tests
        #region Test Methods
        public static ArrayList GetPageList(ArrayList bookmarks)
        {
            if(bookmarks == null)
                return null;
            ArrayList al = null;
            foreach(Hashtable bookmark in bookmarks) {
                if(al == null)
                    al = new ArrayList();
                int iPage;
                if((iPage = GetPageNumber(bookmark)) > 0)
                    al.Add(iPage);
                if(bookmark.ContainsKey("Kids")) {
                    ArrayList kids = bookmark["Kids"] as ArrayList;
                    if(kids != null) {
                        ArrayList pageList = GetPageList(kids);
                        if(pageList != null)
                            al.AddRange(pageList);
                    }
                }
            }
            return al;
        }

        public static ArrayList GetTitleList(ArrayList bookmarks)
        {
            if(bookmarks == null)
                return null;
            ArrayList al = null;
            foreach(Hashtable bookmark in bookmarks) {
                if(al == null)
                    al = new ArrayList();
                string sTitle = "";
                if(bookmark.ContainsKey("Action") && "GoTo".Equals(bookmark["Action"]))
                    sTitle = (string)bookmark["Title"];
                al.Add(sTitle);
                if(bookmark.ContainsKey("Kids")) {
                    ArrayList kids = bookmark["Kids"] as ArrayList;
                    if(kids != null) {
                        ArrayList titleList = GetTitleList(kids);
                        if(titleList != null)
                            al.AddRange(titleList);
                    }
                }
            }
            return al;
        }
        #endregion Test Methods
    }
}
