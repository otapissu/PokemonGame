/// <summary>파괴 직전 포켓몬 상태를 저장하는 스냅샷.</summary>
public class RevivalSnapshot
{
    public PokemonData rootData;
    public PokemonData data;
    public int         enhanceLevel;
    public Gender      gender;
    public bool        isShiny;
    public int         formIndex;
    public int         destroyedAtLevel; // 파괴 당시 강화 단계 (비용 계산용)

    public RevivalSnapshot(PokemonInstance instance)
    {
        rootData         = instance.rootData;
        data             = instance.data;
        enhanceLevel     = instance.enhanceLevel;
        gender           = instance.gender;
        isShiny          = instance.isShiny;
        formIndex        = instance.formIndex;
        destroyedAtLevel = instance.enhanceLevel;
    }
}
