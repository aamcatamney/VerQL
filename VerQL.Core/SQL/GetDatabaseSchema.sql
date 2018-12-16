-- 1. Schema
select s.name as [Name]
from sys.schemas s
where s.schema_id < 16000
and s.name not in ('dbo', 'guest', 'INFORMATION_SCHEMA', 'sys')
order by s.name

-- 2. UserType
select s.name as [Schema], 
	   t.name as [Name], 
	   st.name as [Type], 
	   case
	    when t.max_length = -1 then t.max_length
		when st.name in ('time') then t.scale
		when st.name in ('float') then t.precision
		when st.name in ('ntext', 'nchar', 'nvarchar') then t.max_length / 2
		when st.name in ('text', 'char', 'varchar', 'binary', 'varbinary') then t.max_length
		else 0 end as [MaxLength],
	   t.is_nullable as [IsNullable]
from sys.types t
join sys.schemas s on s.schema_id = t.schema_id
join sys.types st on st.user_type_id = t.system_type_id
where t.is_user_defined = 1
order by s.name, t.name

-- 3. Table
select s.name as [Schema], t.name as [Name]
from sys.tables t
join sys.schemas s on s.schema_id = t.schema_id
where t.is_ms_shipped = 0
order by s.name, t.name

-- 4. DBColumn
select s.name as [Schema], 
	   t.name as [Table], 
	   c.name as [Name], 
	   uty.name as [Type], 
	   case
	    when c.max_length = -1 then c.max_length
		when uty.name in ('time') then c.scale
		when uty.name in ('float') then c.precision
		when uty.name in ('ntext', 'nchar', 'nvarchar') then c.max_length / 2
		when uty.name in ('text', 'char', 'varchar', 'binary', 'varbinary') then c.max_length
		else 0 end as [MaxLength],
	   c.is_nullable as [IsNullable], 
	   c.is_computed as [IsComputed],
	   cc.definition as [ComputedText],
	   cast(case when pkc.object_id is not null then 1 else 0 end as bit) as [IsPrimaryKey],
	   cast(case when ui.object_id is not null then 1 else 0 end as bit) as [IsUnique],
	   c.is_identity as [IsIdentity], 
	   ic.seed_value as [SeedValue],
	   ic.increment_value as [IncrementValue],
	   cast(case when c.user_type_id <> c.system_type_id then 1 else 0 end as bit) as [IsUserDefined],
	   cast(case when dc.object_id is not null then 1 else 0 end as bit) as [HasDefault],
	   dc.definition as [DefaultText]
from sys.columns c
join sys.tables t on t.object_id = c.object_id
join sys.schemas s on s.schema_id = t.schema_id
join sys.types uty on c.user_type_id = uty.user_type_id
left join sys.computed_columns cc on cc.column_id = c.column_id and cc.object_id = c.object_id
left join sys.index_columns pic on pic.column_id = c.column_id and pic.object_id = c.object_id
left join sys.indexes pi on pi.object_id = c.object_id and pi.index_id = pic.index_id and pi.is_primary_key = 1
left join sys.key_constraints pkc on pkc.parent_object_id = c.object_id and pi.index_id = pkc.unique_index_id and pkc.type = 'PK' and pkc.is_system_named = 1
left join sys.index_columns uic on uic.column_id = c.column_id and uic.object_id = c.object_id
left join sys.indexes ui on ui.object_id = c.object_id and ui.index_id = uic.index_id and ui.is_unique_constraint = 1
left join sys.identity_columns ic on ic.object_id = c.object_id and ic.column_id = c.column_id
left join sys.default_constraints dc on dc.parent_object_id = c.object_id and dc.parent_column_id = c.column_id
where t.is_ms_shipped = 0
order by s.name, t.name, c.column_id

-- 5. DBForeignKeyConstraint
select s.name as [Schema], 
	   t.name as [Table], 
	   fk.name as [Name],
	   fk.delete_referential_action_desc as [OnDelete],
	   fk.update_referential_action_desc as [OnUpdate],
	   s.name as [ReferenceSchema], 
	   t.name as [ReferenceTable]
from sys.foreign_keys fk
join sys.tables t on t.object_id = fk.parent_object_id
join sys.schemas s on s.schema_id = t.schema_id
join sys.tables rt on rt.object_id = fk.referenced_object_id
join sys.schemas rs on rs.schema_id = rt.schema_id
order by s.name, t.name, fk.name

-- 6. FBForeignKeyColumn
select s.name as [Schema], 
	   t.name as [Table], 
	   fk.name as [FK],
	   c.name as [Column],
	   rc.name as [ReferenceColumn]
from sys.foreign_key_columns fkc
join sys.foreign_keys fk on fk.object_id = fkc.constraint_object_id
join sys.tables t on t.object_id = fk.parent_object_id
join sys.schemas s on s.schema_id = t.schema_id
join sys.columns c on c.object_id = fkc.parent_object_id and c.column_id = fkc.parent_column_id
join sys.columns rc on rc.object_id = fkc.referenced_object_id and rc.column_id = fkc.referenced_column_id
order by s.name, t.name, fk.name

-- 7. DBPrimaryKeyConstraint
select s.name as [Schema], 
	   t.name as [Table], 
	   pkc.name as [Name],
	   cast(case when i.type = 1 then 1 else 0 end as bit) as [Clustered],
	   i.fill_factor as [FillFactor]
from sys.key_constraints pkc 
join sys.tables t on t.object_id = pkc.parent_object_id
join sys.schemas s on s.schema_id = t.schema_id
join sys.indexes i on i.name = pkc.name and i.index_id = pkc.unique_index_id and i.is_primary_key = 1
where pkc.type = 'PK' 
and pkc.is_system_named = 0
and pkc.is_ms_shipped = 0
order by s.name, t.name

-- 8. DBPrimaryKeyColumn
select s.name as [Schema], 
	   t.name as [Table], 
	   pkc.name as [PK],
	   c.name as [Name],
	   cast(case when ic.is_descending_key = 0 then 1 else 0 end as bit) as [Asc]
from sys.key_constraints pkc 
join sys.tables t on t.object_id = pkc.parent_object_id
join sys.schemas s on s.schema_id = t.schema_id
join sys.indexes i on i.name = pkc.name and i.index_id = pkc.unique_index_id and i.is_primary_key = 1
join sys.index_columns ic on ic.object_id = i.object_id and ic.index_id = i.index_id
join sys.columns c on c.column_id = ic.column_id and ic.object_id = c.object_id
where pkc.type = 'PK' 
and pkc.is_system_named = 0
and pkc.is_ms_shipped = 0
order by s.name, t.name

-- 9. DBUniqueConstraint
select s.name as [Schema], 
	   t.name as [Table], 
	   pkc.name as [Name],
	   cast(case when i.type = 1 then 1 else 0 end as bit) as [Clustered]
from sys.key_constraints pkc 
join sys.tables t on t.object_id = pkc.parent_object_id
join sys.schemas s on s.schema_id = t.schema_id
join sys.indexes i on i.name = pkc.name and i.index_id = pkc.unique_index_id and i.is_unique_constraint = 1
where pkc.type = 'UQ' 
and pkc.is_system_named = 0
and pkc.is_ms_shipped = 0
order by s.name, t.name

-- 10. DBUniqueColumn
select s.name as [Schema], 
	   t.name as [Table], 
	   pkc.name as [UQ],
	   c.name as [Name],
	   cast(case when ic.is_descending_key = 0 then 1 else 0 end as bit) as [Asc]
from sys.key_constraints pkc 
join sys.tables t on t.object_id = pkc.parent_object_id
join sys.schemas s on s.schema_id = t.schema_id
join sys.indexes i on i.name = pkc.name and i.index_id = pkc.unique_index_id and i.is_unique_constraint = 1
join sys.index_columns ic on ic.object_id = i.object_id and ic.index_id = i.index_id
join sys.columns c on c.column_id = ic.column_id and ic.object_id = c.object_id
where pkc.type = 'UQ' 
and pkc.is_system_named = 0
and pkc.is_ms_shipped = 0
order by s.name, t.name

-- 11. Procedure
select s.name as [Schema], 
	   p.name as [Name],
	   m.definition as [Definition]
from sys.procedures p
join sys.schemas s on s.schema_id = p.schema_id
join sys.sql_modules m on m.object_id = p.object_id
where p.is_ms_shipped = 0
order by s.name, p.name

-- 12. Views
select s.name as [Schema], 
	   v.name as [Name],
	   m.definition as [Definition]
from sys.views v
join sys.schemas s on s.schema_id = v.schema_id
join sys.sql_modules m on m.object_id = v.object_id
where v.is_ms_shipped = 0
order by s.name, v.name

-- 13. Function
select s.name as [Schema], 
	   o.name as [Name],
	   m.definition as [Definition]
from sys.all_objects o
join sys.schemas s on s.schema_id = o.schema_id
join sys.sql_modules m on m.object_id = o.object_id
where o.type in ('TF', 'FN', 'IF')
and o.is_ms_shipped = 0
order by s.name, o.name

-- 14. Trigger
select s.name as [Schema],
	   t.name as [Name],
	   m.definition as [Definition]
from sys.triggers t
join sys.all_objects o on o.object_id = t.object_id
join sys.schemas s on s.schema_id = o.schema_id
join sys.sql_modules m on m.object_id = t.object_id
where t.is_ms_shipped = 0
order by t.name