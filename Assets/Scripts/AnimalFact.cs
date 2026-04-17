using UnityEngine;

[CreateAssetMenu(fileName = "NewAnimalFact", menuName = "Education/AnimalFact")]
public class AnimalFact : ScriptableObject
{
    public string animalName;

    [TextArea(3, 6)]
    public string educationalText;
}