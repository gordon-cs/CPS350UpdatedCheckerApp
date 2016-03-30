select a.validine_id, a.firstname, a.lastname
  from Attendum.dbo.collectors as c
  inner join
  Attendum.dbo.account as a
  on c.sCollectorAltPin = a.gordon_id

