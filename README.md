
## PageSelector

PageSelector implements an iterator that enables the caller to iterate

through selected pages in a PDF file. For example:

  PageSelector ps = new PageSelector() {

                            ParitySelection = ParitySelection.Even;

                            BookmarkSelection = BookmarkSelection.MatchContains;

                            BookmarkString = "media" 

                            PageSelection = PageSelection.PageRanges,

                            PageRanges = "1-10,30-46,50" 

                     };

will produce an iterator that will return the pages in a PDF file that are

even, in the range "1-10,30-46,50", and have bookmarks that contain the

text "media".



For clarity, input validation, error handling, and logging have been largely omitted.



