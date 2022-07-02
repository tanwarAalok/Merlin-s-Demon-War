using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class Card : MonoBehaviour
{
    public CardData cardData = null;
    public TMP_Text titleText = null;
    public TMP_Text descriptionText = null;
    public Image damageImage = null;
    public Image costImage = null;
    public Image cardImage = null;
    public Image frameImage = null;
    public Image burnImage = null;

    public void Initialize(){
        if(cardData == null){
            Debug.LogError("Card has no CardData!");
            return;
        }
        titleText.text = cardData.cardTitle;
        descriptionText.text = cardData.description;
        cardImage.sprite = cardData.cardImage;
        frameImage.sprite = cardData.frameImage;
        costImage.sprite = GameController.instance.healthNumbers[cardData.cost];
        damageImage.sprite = GameController.instance.damageNumbers[cardData.damage];
        
    }
}
