
## PageSelector

PageSelector implements an iterator that enables the caller to iterate <br/>
through selected pages in a PDF file. For example: <br/>
  PageSelector ps = new PageSelector() { <br/>
                            ParitySelection = ParitySelection.Even; <br/>
                            BookmarkSelection = BookmarkSelection.MatchContains; <br/>
                            BookmarkString = "media"  <br/>
                            PageSelection = PageSelection.PageRanges, <br/>
                            PageRanges = "1-10,30-46,50"  <br/>
                     }; <br/>
will produce an iterator that will return the pages in a PDF file that are <br/>
even, in the range "1-10,30-46,50", and have bookmarks that contain the <br/>
text "media". <br/>



For clarity, input validation, error handling, and logging have been largely omitted.



