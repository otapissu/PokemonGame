using UnityEngine;

[CreateAssetMenu(menuName = "Pokemon/PokemonData")]
public class PokemonData : ScriptableObject
{
    [Header("Identity")]
    public int id;
    public string pokemonName;
    public bool canHatch = true;
    public bool isLegendary;

    [Header("Gender")]
    public GenderVisualType genderVisualType;
    [Range(0f, 1f)]
    public float maleRatio = 0.5f;

    [Header("Evolution")]
    public EvolutionOption[] evolutionOptions;

    public bool HasEvolution()
    {
        if (evolutionOptions == null)
        {
            return false;
        }

        if (evolutionOptions.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < evolutionOptions.Length; i++)
        {
            EvolutionOption option = evolutionOptions[i];

            if (option == null)
            {
                continue;
            }

            if (option.targetData != null)
            {
                return true;
            }
        }

        return false;
    }
}