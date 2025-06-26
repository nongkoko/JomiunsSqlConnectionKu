using JomiunsCom;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using System;
using System.Data.Common;
using System.Linq;

namespace netCore_sqlConnectionKu
{
    internal interface iSelectQueryPart02
    {
        object criteriaAND { get; }
        enDatabaseType databaseType { get; }
    }

    internal class selectQueryPart02Resolver(iSelectQueryPart02 ppppp)
    {
        internal (string whereCriteria_result, DbParameter[] listParam_result) solvePart02()
        {
            if (ppppp.criteriaAND == null)
                return (null, null);

            var m12986 = ppppp.criteriaAND.GetType().GetProperties().Select(oo => new
            {
                columnName = oo.Name,
                parameter = ppppp.databaseType == enDatabaseType.SQLServer ?
                            (DbParameter)new SqlParameter($"@{oo.Name}", oo.GetValue(ppppp.criteriaAND, null) ?? DBNull.Value) :
                            (DbParameter)new SqliteParameter($"@{oo.Name}", oo.GetValue(ppppp.criteriaAND, null) ?? DBNull.Value)

            });

            var p58125n = m12986.Select(ooo =>
            {
                if (ooo.parameter.Value == DBNull.Value)
                    return $"{ooo.columnName} is null";
                return $"{ooo.columnName} = {ooo.parameter.ParameterName}";
            }).ToArray();
            var j513286 = string.Join(" and ", p58125n);
            var h985612 = m12986.Select(ooo => ooo.parameter)
                                .Where(oo => oo.Value != DBNull.Value)
                                .ToArray();
            return (j513286, h985612);
        }
    }
}
