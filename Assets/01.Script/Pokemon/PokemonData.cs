using UnityEngine;

public enum Gender
{
    None,
    Male,
    Female
}

public enum GenderVisualType
{
    None,           // 성별 없음
    SameVisual,     // 암수 동일 외형
    DifferentVisual // 암수 외형 다름
}

[CreateAssetMenu(menuName = "Pokemon/PokemonData")]
public class PokemonData : ScriptableObject
{
    public int id;
    public string pokemonName;
    public bool canHatch = true;
    public bool isLegendary;

    [Header("Gender")]
    public GenderVisualType genderVisualType;
    [Range(0f, 1f)]
    public float maleRatio = 0.5f;

    [Header("Evolution")]
    public PokemonData nextEvolution;
    public PokemonData secondEvolution;
}