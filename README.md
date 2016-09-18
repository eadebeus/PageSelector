
## PageSelector

PageSelector implements an iterator that enables the caller to iterate <br/>
through selected pages in a PDF file. For example: <br/>
&nbsp;&nbsp;PageSelector ps = new PageSelector() { <br/>
&nbsp;&nbsp;&nbsp;&nbsp;ParitySelection = ParitySelection.Even; <br/>
&nbsp;&nbsp;&nbsp;&nbsp;BookmarkSelection = BookmarkSelection.MatchContains; <br/>
&nbsp;&nbsp;&nbsp;&nbsp;BookmarkString = "media"  <br/>
&nbsp;&nbsp;&nbsp;&nbsp;PageSelection = PageSelection.PageRanges, <br/>
&nbsp;&nbsp;&nbsp;&nbsp;PageRanges = "1-10,30-46,50"  <br/>
&nbsp;&nbsp;}; <br/>
will produce an iterator that will return the pages in a PDF file that are <br/>
even, in the range "1-10,30-46,50", and have bookmarks that contain the <br/>
text "media". <br/>



For clarity, input validation, error handling, and logging have been largely omitted.



