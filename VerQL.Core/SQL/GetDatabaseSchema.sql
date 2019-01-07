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
select s.name as [TableSchema],
	t.name as [TableName],
	c.name as [Name],
	c.column_id as [Sequence],
	case when c.user_type_id <> c.system_type_id then utys.name + '.' + uty.name else uty.name end as [Type],
	case
		when uty.name in ('xml') then 0
	  when c.max_length = -1 then c.max_length
		when uty.name in ('time') then c.scale
		when uty.name in ('float') then c.precision
		when uty.name in ('ntext', 'nchar', 'nvarchar') then c.max_length / 2
		when uty.name in ('text', 'char', 'varchar', 'binary', 'varbinary') then c.max_length
		else 0 end as [MaxLength],
	c.is_nullable as [IsNullable],
	c.is_computed as [IsComputed],
	cc.definition as [ComputedText],
	c.is_identity as [IsIdentity],
	ic.seed_value as [SeedValue],
	ic.increment_value as [IncrementValue],
	cast(case when c.user_type_id <> c.system_type_id then 1 else 0 end as bit) as [IsUserDefined],
	cast(case when dc.object_id is not null then 1 else 0 end as bit) as [HasDefault],
	case when dc.is_system_named = 0 then dc.name else null end as [DefaultName],
	dc.definition as [DefaultText]
from sys.columns c
	join sys.tables t on t.object_id = c.object_id
	join sys.schemas s on s.schema_id = t.schema_id
	join sys.types uty on c.user_type_id = uty.user_type_id
	join sys.schemas utys on utys.schema_id = uty.schema_id
	left join sys.computed_columns cc on cc.column_id = c.column_id and cc.object_id = c.object_id
	left join sys.identity_columns ic on ic.object_id = c.object_id and ic.column_id = c.column_id
	left join sys.default_constraints dc on dc.parent_object_id = c.object_id and dc.parent_column_id = c.column_id
where t.is_ms_shipped = 0
order by s.name, t.name, c.column_id

-- 5. DBForeignKeyConstraint
select s.name as [TableSchema],
	t.name as [TableName],
	fk.name as [Name],
	fk.delete_referential_action_desc as [OnDelete],
	fk.update_referential_action_desc as [OnUpdate],
	s.name as [ReferenceSchema],
	t.name as [ReferenceTable],
	fk.is_system_named as [SystemNamed]
from sys.foreign_keys fk
	join sys.tables t on t.object_id = fk.parent_object_id
	join sys.schemas s on s.schema_id = t.schema_id
	join sys.tables rt on rt.object_id = fk.referenced_object_id
	join sys.schemas rs on rs.schema_id = rt.schema_id
order by s.name, t.name, fk.name

-- 6. FBForeignKeyColumn
select s.name as [TableSchema],
	t.name as [TableName],
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
select s.name as [TableSchema],
	t.name as [TableName],
	pkc.name as [Name],
	cast(case when i.type = 1 then 1 else 0 end as bit) as [Clustered],
	i.fill_factor as [FillFactor],
	pkc.is_system_named as [SystemNamed]
from sys.key_constraints pkc
	join sys.tables t on t.object_id = pkc.parent_object_id
	join sys.schemas s on s.schema_id = t.schema_id
	join sys.indexes i on i.name = pkc.name and i.index_id = pkc.unique_index_id and i.is_primary_key = 1
where pkc.type = 'PK'
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
	and pkc.is_ms_shipped = 0
order by s.name, t.name

-- 9. DBUniqueConstraint
select s.name as [TableSchema],
	t.name as [TableName],
	pkc.name as [Name],
	cast(case when i.type = 1 then 1 else 0 end as bit) as [Clustered],
	pkc.is_system_named as [SystemNamed]
from sys.key_constraints pkc
	join sys.tables t on t.object_id = pkc.parent_object_id
	join sys.schemas s on s.schema_id = t.schema_id
	join sys.indexes i on i.name = pkc.name and i.index_id = pkc.unique_index_id and i.is_unique_constraint = 1
where pkc.type = 'UQ'
	and pkc.is_ms_shipped = 0
order by s.name, t.name

-- 10. DBUniqueColumn
select s.name as [TableSchema],
	t.name as [TableName],
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

-- 15. Indexs
select s.name as [TableSchema], 
	   t.name as [TableName],  
	   i.name as [Name], 
	   i.is_unique as [IsUnique]
from sys.indexes i
join sys.tables t on t.object_id = i.object_id
join sys.schemas s on s.schema_id = t.schema_id
where i.type = 2
and i.is_unique_constraint = 0
and i.is_primary_key = 0
order by s.name, t.name, i.index_id

-- 16. Index Columns
select s.name as [TableSchema], 
	   t.name as [TableName],  
	   i.name as [IndexName], 
	   c.name as [Name],
	   cast(case when ic.is_descending_key = 0 then 1 else 0 end as bit) as [Asc],
	   ic.is_included_column as [Included]
from sys.index_columns ic
join sys.columns c on c.object_id = ic.object_id and c.column_id = ic.column_id
join sys.indexes i on ic.object_id = i.object_id and ic.index_id = i.index_id
join sys.tables t on t.object_id = i.object_id
join sys.schemas s on s.schema_id = t.schema_id
where i.type = 2
and i.is_unique_constraint = 0
and i.is_primary_key = 0
order by s.name, t.name, i.index_id, ic.index_column_id

-- 17. Extended Properties
select ep.name as [Name],
       ep.value as [Value],
	   'SCHEMA' as [Level0Type],
	   s.name as [Level0Name],
	   case
	     when ep.class = 1 and ao.type = 'U' then 'TABLE'
		 when ep.class = 1 and ao.type = 'V' then 'VIEW'
		 when ep.class = 6 then 'TYPE'
	   end as [Level1Type],
	   case
		 when ep.class = 1 then ao.name
	     when ep.class = 6 then t.name
	   end as [Level1Name],
	   case
	     when ep.class = 1 and c.column_id is not null then 'COLUMN'
	   end as [Level2Type],
	   c.name as [Level2Name]
from sys.extended_properties ep
left join sys.all_objects ao on ep.class in (1,3,7) and ao.object_id = ep.major_id
left join sys.types t on ep.class = 6 and t.user_type_id = ep.major_id
left join sys.columns c on ep.class = 1 and c.object_id = ep.major_id and c.column_id = ep.minor_id
left join sys.schemas s on s.schema_id = ao.schema_id or s.schema_id = t.schema_id
where ep.class in (1,3,6,7)