use TesseraTest;

select * from Library;
select * from AspNetUsers;

delete from Library
where Title like '%Test Book%';

delete from AspNetUsers
where FirstName = 'TestDummy';