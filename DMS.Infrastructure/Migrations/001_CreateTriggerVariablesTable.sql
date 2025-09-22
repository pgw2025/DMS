-- 创建触发器与变量关联表
CREATE TABLE TriggerVariables (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TriggerDefinitionId INTEGER NOT NULL,
    VariableId INTEGER NOT NULL,
    FOREIGN KEY (TriggerDefinitionId) REFERENCES TriggerDefinitions(Id) ON DELETE CASCADE,
    FOREIGN KEY (VariableId) REFERENCES Variables(Id) ON DELETE CASCADE
);

-- 创建索引以提高查询性能
CREATE INDEX IX_TriggerVariables_TriggerDefinitionId ON TriggerVariables (TriggerDefinitionId);
CREATE INDEX IX_TriggerVariables_VariableId ON TriggerVariables (VariableId);

-- 迁移现有数据：将TriggerDefinitions表中的VariableId迁移到新的关联表中
INSERT INTO TriggerVariables (TriggerDefinitionId, VariableId)
SELECT Id, VariableId
FROM TriggerDefinitions
WHERE VariableId IS NOT NULL AND VariableId > 0;

-- 删除TriggerDefinitions表中的VariableId列（如果数据库支持）
-- 注意：SQLite不支持直接删除列，所以这里只是注释说明
-- 在支持的数据库中，可以使用以下语句：
-- ALTER TABLE TriggerDefinitions DROP COLUMN VariableId;