namespace Osmy.Models.Sbom
{
    /// <summary>
    /// パッケージ
    /// </summary>
    /// <param name="Name">名前</param>
    /// <param name="Version">バージョン</param>
    /// <param name="IsRootPackage">ルートパッケージか</param>
    public abstract record Package(string Name, string Version, bool IsRootPackage);
}
