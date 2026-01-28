using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class Acheivement : ScriptableObject
{

    public string Name;
    public string Description;
    public Accomplishments.AccomplishmentName[] tiedAccomplishments;
    public int[] numAccomplishmentsRequired;

    public bool obtained;



}
