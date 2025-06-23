namespace netCore_sqlConnectionKu
{
#if NET6_0_OR_GREATER

    public interface iSomething
    {
        string tableName { get; }
        bool IsWithNolock { get; }
        string[] columnNameToSelect { get; }
    }

    internal class querySelectPart01Resolver(iSomething hehe)
    {
        internal string solvePart01()
        {
            var mkjh3 = (hehe.columnNameToSelect == null || hehe.columnNameToSelect.Length < 1) ? "*" : string.Join(",", hehe.columnNameToSelect);
            var m195b25 = hehe.IsWithNolock ? "with (nolock)" : "";
            var part01 = $@"
select {mkjh3}
from dbo.{hehe.tableName} {m195b25}
";
            return part01;
        }
    }
#endif
}
