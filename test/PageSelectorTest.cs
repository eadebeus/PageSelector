using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PageSelectorDemo;
using iTextSharp.text.pdf;


namespace PageSelectorTestNamespace
{
    public class SourceFileFactory
    {
        private static SourceFileFactory _instance = new SourceFileFactory();
        private SourceFileFactory() { }
        public static SourceFileFactory Instance { get { return _instance; } }

        private static bool _mock = false;

        public ISourceFile GetSourceFile()
        {
            if(_mock)
                return new MockSourceFile();
            else
                return new PDFSourceFile();
        }
    }

    [TestClass]
    public class PageSelectorTest
    {
        public PageSelectorTest()
        {
        }

        private static string _basePath = null;

        [ClassInitialize()]
        public static void InitBasePathAndDTDFolder(TestContext testContext)
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            Assert.IsNotNull(assembly);

            // Convert "...\test\bin\" into "...\TestData\"
            _basePath = GetParentDirectory(Path.GetDirectoryName(assembly.Location), 2);
            _basePath = Path.Combine(_basePath, @"TestData");

            Assert.IsNotNull(_basePath);
            Assert.IsTrue(Directory.Exists(_basePath));
        }

        private static string GetParentDirectory(string path, int parentCount)
        {
            if(string.IsNullOrEmpty(path) || parentCount < 1)
                return path;

            string parent = Path.GetDirectoryName(path);

            if(--parentCount > 0)
                return GetParentDirectory(parent, parentCount);

            return parent;
        }

        public static string GetPathToSourceFile(string fileName) 
        {
            var sourceDir = Path.GetFullPath(_basePath);
            string sourceFilePath = null;
            if(fileName != null) {
                sourceFilePath = Path.Combine(sourceDir, fileName);
                Assert.IsTrue(File.Exists(sourceFilePath));
            }
            else 
                sourceFilePath = sourceDir;
            return sourceFilePath;
        }

        [TestMethod]
        public void PageSelectorPageRangeTest()
        {
            string sInFile = GetPathToSourceFile("TestCase.pdf");

            ISourceFile sourceFile = SourceFileFactory.Instance.GetSourceFile();
            Assert.IsTrue(sourceFile.OpenFile(sInFile));

            PageSelector ps = new PageSelector();
            ps.ReadFileInformation(sourceFile);
            string pageRanges = "1-4,7";
            ps.SetPageRanges(pageRanges);
            
            // Check GetPages()
            List<int> expected = new List<int> { 1, 2, 3, 4, 7 };
            List<int> received = new List<int>();
            foreach(int i in ps.GetPages())
                received.Add(i);
            Assert.IsTrue(expected.SequenceEqual(received));

            // Check Enumerator
            IEnumerator iter = ps.GetEnumerator();
            Assert.IsTrue(iter.MoveNext());
            Assert.AreEqual(1, (int)iter.Current);
            Assert.IsTrue(iter.MoveNext());
            Assert.AreEqual(2, (int)iter.Current);
            Assert.IsTrue(iter.MoveNext());
            Assert.AreEqual(3, (int)iter.Current);
            Assert.IsTrue(iter.MoveNext());
            Assert.AreEqual(4, (int)iter.Current);
            Assert.IsTrue(iter.MoveNext());
            Assert.AreEqual(7, (int)iter.Current);
            Assert.IsFalse(iter.MoveNext());

            sourceFile.CloseFile();
        }

        [TestMethod]
        public void PageSelectorOrientationTest()
        {
            string sInFile = GetPathToSourceFile("TestCase.pdf");

            ISourceFile sourceFile = SourceFileFactory.Instance.GetSourceFile();
            Assert.IsTrue(sourceFile.OpenFile(sInFile));

            PageSelector ps = new PageSelector();
            ps.ReadFileInformation(sourceFile);
            ps.OrientationSelection = OrientationSelection.Landscape;
            
            // Find landscape pages (should be only page 9)
            IEnumerator iter = ps.GetEnumerator();
            iter.MoveNext();
            int iPage = (int)iter.Current;
            Assert.AreEqual(9, iPage);
            Assert.IsFalse(iter.MoveNext());

            // Find portrait pages (should be all but page 9)
            ps.OrientationSelection = OrientationSelection.Portrait;
            List<int> expected = Enumerable.Range(1, 22).ToList();
            expected.Remove(9);
            List<int> received = new List<int>();
            foreach(int i in ps.GetPages())
                received.Add(i);
            Assert.IsTrue(expected.SequenceEqual(received));

            sourceFile.CloseFile();
        }

        [TestMethod]
        public void PageSelectorParityTest()
        {
            string sInFile = GetPathToSourceFile("TestCase.pdf");

            ISourceFile sourceFile = SourceFileFactory.Instance.GetSourceFile();
            Assert.IsTrue(sourceFile.OpenFile(sInFile));

            PageSelector ps = new PageSelector();
            ps.ReadFileInformation(sourceFile);               

            // Find pages that are Even
            ps.PageSelection = PageSelection.All;
            ps.ParitySelection = ParitySelection.Even;

            List<int> expected = Enumerable.Range(1, 22).Where(x => (x % 2) == 0).ToList();
            List<int> received = new List<int>();
            foreach(int i in ps.GetPages())
                received.Add(i);
            Assert.IsTrue(expected.SequenceEqual(received));

            // Find pages that are Odd
            ps.PageSelection = PageSelection.All;
            ps.ParitySelection = ParitySelection.Odd;

            expected = Enumerable.Range(1, 22).Where(x => (x % 2) != 0).ToList();
            received = new List<int>();
            foreach(int i in ps.GetPages())
                received.Add(i);
            Assert.IsTrue(expected.SequenceEqual(received));

            sourceFile.CloseFile();
        }

        [TestMethod]
        public void PageSelectorOrientationAndParityTest()
        {
            string sInFile = GetPathToSourceFile("TestCase.pdf");

            ISourceFile sourceFile = SourceFileFactory.Instance.GetSourceFile();
            Assert.IsTrue(sourceFile.OpenFile(sInFile));

            PageSelector ps = new PageSelector();
            ps.ReadFileInformation(sourceFile);

            // Find pages that are both Odd and Portrait 
            ps.PageSelection = PageSelection.All;
            ps.ParitySelection = ParitySelection.Odd;
            ps.OrientationSelection = OrientationSelection.Portrait;

            List<int> expected = Enumerable.Range(1, 22).Where(x => (x % 2) != 0).ToList();
            expected.Remove(9);
            List<int> received = new List<int>();
            foreach(int i in ps.GetPages())
                received.Add(i);
            Assert.IsTrue(expected.SequenceEqual(received));

            sourceFile.CloseFile();
        }

        [TestMethod]
        public void PageSelectorBookmarksAnyTest()
        {
            string sInFile = GetPathToSourceFile("TestCase.pdf");

            ISourceFile sourceFile = SourceFileFactory.Instance.GetSourceFile();
            Assert.IsTrue(sourceFile.OpenFile(sInFile));

            PageSelector ps = new PageSelector();
            ps.ReadFileInformation(sourceFile);

            // Find pages that have bookmarks
            ps.BookmarkSelection = BookmarkSelection.MatchAny;

            List<int> expected = new List<int>() { 4, 8, 9, 14, 15, 22 };
            List<int> received = new List<int>();
            foreach(int i in ps.GetPages())
                received.Add(i);
            Assert.IsTrue(expected.SequenceEqual(received));

            sourceFile.CloseFile();
        }

        [TestMethod]
        public void PageSelectorBookmarksContainsTest()
        {
            string sInFile = GetPathToSourceFile("TestCase.pdf");

            ISourceFile sourceFile = SourceFileFactory.Instance.GetSourceFile();
            Assert.IsTrue(sourceFile.OpenFile(sInFile));

            PageSelector ps = new PageSelector();
            ps.ReadFileInformation(sourceFile);

            // Find pages that have bookmarks which contain the match string
            ps.BookmarkSelection = BookmarkSelection.MatchContains;
            ps.BookmarkString = "Staple";

            List<int> expected = new List<int>() { 4, 8 };
            List<int> received = new List<int>();
            foreach(int i in ps.GetPages())
                received.Add(i);
            Assert.IsTrue(expected.SequenceEqual(received));

            sourceFile.CloseFile();
        }

        [TestMethod]
        public void PageSelectorBookmarksNotContainTest()
        {
            string sInFile = GetPathToSourceFile("TestCase.pdf");

            ISourceFile sourceFile = SourceFileFactory.Instance.GetSourceFile();
            Assert.IsTrue(sourceFile.OpenFile(sInFile));

            PageSelector ps = new PageSelector();
            ps.ReadFileInformation(sourceFile);

            // Find pages that have bookmarks, but which has bookmarks that do not contain the match string
            ps.BookmarkSelection = BookmarkSelection.MatchDoesNotContain;
            ps.BookmarkString = "Staple";

            List<int> expected = new List<int>() { 8, 9, 14, 15, 22 };
            List<int> received = new List<int>();
            foreach(int i in ps.GetPages())
                received.Add(i);
            Assert.IsTrue(expected.SequenceEqual(received));

            sourceFile.CloseFile();
        }

        [TestMethod]
        public void PageSelectorBookmarksEqualsTest()
        {
            string sInFile = GetPathToSourceFile("TestCase.pdf");

            ISourceFile sourceFile = SourceFileFactory.Instance.GetSourceFile();
            Assert.IsTrue(sourceFile.OpenFile(sInFile));

            PageSelector ps = new PageSelector();
            ps.ReadFileInformation(sourceFile);

            // Find pages that have bookmarks that are equal to the match string
            ps.BookmarkSelection = BookmarkSelection.MatchEquals;
            ps.BookmarkString = "<<  /Jog 3 >> setpagedevice";

            List<int> expected = new List<int>() { 14, 22 };
            List<int> received = new List<int>();
            foreach(int i in ps.GetPages())
                received.Add(i);
            Assert.IsTrue(expected.SequenceEqual(received));

            sourceFile.CloseFile();
        }

        [TestMethod]
        public void PageSelectorBookmarksNotEqualTest()
        {
            string sInFile = GetPathToSourceFile("TestCase.pdf");

            ISourceFile sourceFile = SourceFileFactory.Instance.GetSourceFile();
            Assert.IsTrue(sourceFile.OpenFile(sInFile));

            PageSelector ps = new PageSelector();
            ps.ReadFileInformation(sourceFile);

            // Find pages that have bookmarks, but which has bookmarks that are not equal to the match string
            ps.BookmarkSelection = BookmarkSelection.MatchNotEqual;
            ps.BookmarkString = "<<  /Jog 3 >> setpagedevice";

            List<int> expected = new List<int>() { 4, 8, 9, 14, 15 };
            List<int> received = new List<int>();
            foreach(int i in ps.GetPages())
                received.Add(i);
            Assert.IsTrue(expected.SequenceEqual(received));

            sourceFile.CloseFile();
        }

        [TestMethod]
        public void PageSelectorEvenAndBookmarksNotEqualTest()
        {
            string sInFile = GetPathToSourceFile("TestCase.pdf");

            ISourceFile sourceFile = SourceFileFactory.Instance.GetSourceFile();
            Assert.IsTrue(sourceFile.OpenFile(sInFile));

            PageSelector ps = new PageSelector();
            ps.ReadFileInformation(sourceFile);

            // Find pages that are Even, have bookmarks, but whose bookmarks are not equal to the match string
            ps.ParitySelection = ParitySelection.Even;
            ps.BookmarkSelection = BookmarkSelection.MatchNotEqual;
            ps.BookmarkString = "<<  /Jog 3 >> setpagedevice";

            List<int> expected = new List<int>() { 4, 8, 14 };
            List<int> received = new List<int>();
            foreach(int i in ps.GetPages())
                received.Add(i);
            Assert.IsTrue(expected.SequenceEqual(received));

            sourceFile.CloseFile();
        }

        [TestMethod]
        public void PageSelectorEvenAndPageRangeAndBookmarksNotEqualTest()
        {
            string sInFile = GetPathToSourceFile("TestCase.pdf");

            ISourceFile sourceFile = SourceFileFactory.Instance.GetSourceFile();
            Assert.IsTrue(sourceFile.OpenFile(sInFile));

            PageSelector ps = new PageSelector();
            ps.ReadFileInformation(sourceFile);

            // Find pages that are Even, have bookmarks and whose bookmarks are not equal to the match string, and are in the page range range 1-4
            ps.ParitySelection = ParitySelection.Even;
            ps.BookmarkSelection = BookmarkSelection.MatchNotEqual;
            ps.BookmarkString = "<<  /Jog 3 >> setpagedevice";
            string pageRanges = "1-9";
            ps.SetPageRanges(pageRanges);

            List<int> expected = new List<int>() { 4, 8 };
            List<int> received = new List<int>();
            foreach(int i in ps.GetPages())
                received.Add(i);
            Assert.IsTrue(expected.SequenceEqual(received));

            sourceFile.CloseFile();
        }

        [TestMethod]
        public void PageSelectorBookmarksEqualsAndPageRangeTest()
        {
            string sInFile = GetPathToSourceFile("TestCase.pdf");

            ISourceFile sourceFile = SourceFileFactory.Instance.GetSourceFile();
            Assert.IsTrue(sourceFile.OpenFile(sInFile));

            PageSelector ps = new PageSelector();
            ps.ReadFileInformation(sourceFile);

            // Find pages that have bookmarks that are equal to the match string, and are in the page range 20-22
            ps.BookmarkSelection = BookmarkSelection.MatchEquals;
            ps.BookmarkString = "<<  /Jog 3 >> setpagedevice";
            string pageRanges = "20-22";
            ps.SetPageRanges(pageRanges);

            List<int> expected = new List<int>() { 22 };
            List<int> received = new List<int>();
            foreach(int i in ps.GetPages())
                received.Add(i);
            Assert.IsTrue(expected.SequenceEqual(received));

            sourceFile.CloseFile();
        }

        [TestMethod]
        public void VerifyBookmarksInTestCase()
        {
            string sInFile = GetPathToSourceFile("TestCase.pdf");

            PdfReader reader = new PdfReader(sInFile);
            if(reader != null) {
                Assert.AreEqual(22, reader.NumberOfPages);

                ArrayList bookmarkList = new ArrayList();
                reader.ConsolidateNamedDestinations();
                ArrayList bookmarks = SimpleBookmark.GetBookmark(reader);
                Assert.IsNotNull(bookmarks);

                ArrayList al = PDFSourceFile.GetPageList(bookmarks);
                Assert.IsNotNull(al);
                Assert.AreEqual(10, al.Count);
                Assert.AreEqual(4, (int)al[0]);
                Assert.AreEqual(8, (int)al[1]);
                Assert.AreEqual(8, (int)al[2]);
                Assert.AreEqual(8, (int)al[3]);
                Assert.AreEqual(9, (int)al[4]);
                Assert.AreEqual(14, (int)al[5]);
                Assert.AreEqual(14, (int)al[6]);
                Assert.AreEqual(14, (int)al[7]);
                Assert.AreEqual(15, (int)al[8]);
                Assert.AreEqual(22, (int)al[9]);

                ArrayList titles = PDFSourceFile.GetTitleList(bookmarks);
                Assert.IsNotNull(titles);
                Assert.AreEqual(10, titles.Count);
                Assert.IsTrue(String.Compare("<<  /Staple 0 /OutputType (Stacker) >> setpagedevice", titles[0].ToString()) == 0);
                Assert.IsTrue(String.Compare("<<  /Staple 0 /OutputType (Stacker) >> setpagedevice", titles[1].ToString()) == 0);
                Assert.IsTrue(String.Compare("<<  /Jog 0 >> setpagedevice", titles[2].ToString()) == 0);
                Assert.IsTrue(String.Compare("<<  /MediaType (cover) /MediaType (cover) /MediaWeight 210.0 /MediaFrontCoating (Glossy) /MediaBackCoating (Glossy) >> setpagedevice", titles[3].ToString()) == 0);
                Assert.IsTrue(String.Compare("<<  /MediaType (bond) /MediaType (bond) /MediaWeight 90.0 /MediaFrontCoating (Any) /MediaBackCoating (Any) >> setpagedevice", titles[4].ToString()) == 0);
                Assert.IsTrue(String.Compare("<<  /Jog 3 >> setpagedevice", titles[5].ToString()) == 0);
                Assert.IsTrue(String.Compare("<<  /Jog 0 >> setpagedevice", titles[6].ToString()) == 0);
                Assert.IsTrue(String.Compare("<<  /MediaType (cover) /MediaType (cover) /MediaWeight 210.0 /MediaFrontCoating (Glossy) /MediaBackCoating (Glossy) >> setpagedevice", titles[7].ToString()) == 0);
                Assert.IsTrue(String.Compare("<<  /MediaType (bond) /MediaType (bond) /MediaWeight 90.0 /MediaFrontCoating (Any) /MediaBackCoating (Any) >> setpagedevice", titles[8].ToString()) == 0);
                Assert.IsTrue(String.Compare("<<  /Jog 3 >> setpagedevice", titles[9].ToString()) == 0);

                reader.Close();
                reader = null;
            }
        }

    }
}
