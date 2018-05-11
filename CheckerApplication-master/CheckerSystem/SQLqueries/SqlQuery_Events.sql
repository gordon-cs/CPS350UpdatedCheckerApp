 select sEventID, sTitle, dtStart, dtEnd
 from Attendum.dbo.Events
 where abs( datediff(day, dtStart, dtEnd) ) <= (31 * 1)
  and dtStart >=  getdate() -7
 order by dtStart