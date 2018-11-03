using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Linq;

public class SC_Build_Cockatrice : MonoBehaviour {

	void Awake() {

		gameObject.SetActive (Directory.Exists("C:\\Users/Ben"));

	}

	public void BuildCockatriceXML() {

		string cockatricePath = "C:\\Users/Ben/AppData/Local/Cockatrice/Cockatrice/";

		XDocument xml = new XDocument (

			new XElement("cockatrice_carddatabase",
			
				new XAttribute("version", 3),

				new XElement("cards")
			
			)

        );

		foreach (string dir in Directory.GetDirectories ("F:\\CFW\\JPG")) {

			string dirN = Path.GetFileName (dir);

			if (dirN != "Other") {

				foreach (string file in Directory.GetFiles(dir)) {

					File.Copy (file, cockatricePath + "pics/CUSTOM/" + Path.GetFileName (file), true);

					xml.Element ("cockatrice_carddatabase").Element("cards").Add (

						new XElement("card",

							new XElement("name", Path.GetFileNameWithoutExtension(file)),

							new XElement("set", "CFW",
							
								new XAttribute("picURL", "")
							
							),

							new XElement("tablerow", 2),

							new XElement("type", (!dirN.Contains("Mythos")) ? dirN.Remove(dirN.Length - 1) : dirN)

						)

					);

				}

			}

		}

		xml.Save (cockatricePath + "customsets/01.CFW.xml");

	}

}
