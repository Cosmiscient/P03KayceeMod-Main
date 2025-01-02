import numpy as np
import requests
import p03_wiki
import os
import json
import base64
import glob
import sys

from PIL import Image
import time
import gzip
import functools


def get_portrait_b64(card_name):
    with open(
        os.path.join(
            os.environ["INSCRYPTION_PATH"], "cardexports", f"card_{card_name}.png"
        ),
        "rb",
    ) as f:
        return base64.b64encode(f.read()).decode("ascii")


def get_ability_b64(ability_name):
    with open(
        os.path.join(
            os.environ["INSCRYPTION_PATH"], "cardexports", f"ability_{ability_name}.png"
        ),
        "rb",
    ) as f:
        return base64.b64encode(f.read()).decode("ascii")


def get_abilities_request_body(data):
    ability_dict = p03_wiki.get_ability_dictionary()
    retval = [{"referenceId": "", "extraData": "$undefined"}] * 5
    for i, ab in enumerate(data["abilities"]):
        ability_data = ability_dict[ab]

        retval[i] = {
            "referenceId": "custom",
            "nameOnCard": "",
            "description": "",
            "gameId": "",
            "imageBase64": "data:image/png;base64,"
            + get_ability_b64(ability_data["name"]),
            "gbcImageBase64": "$undefined",
            "pixelProfilgateImageBase64": "$undefined",
            "augmentedImageBase64": "$undefined",
        }

    return retval


def auto_gemify(card_name):
    return "sentinel" in card_name.lower() or card_name in [
        "GemRipper",
        "TechMoxTriple",
    ]


def is_conduit(card_name):
    data = p03_wiki.get_card_data(card_name)
    ability_dict = p03_wiki.get_ability_dictionary()
    for ab in data["abilities"]:
        ab_data = ability_dict[ab]
        if ab_data["conduit"]:
            return True
    return False


def get_secondary_data(card_name):
    secondary = []
    if auto_gemify(card_name):
        secondary.append("gemified")
    if is_conduit(card_name):
        secondary.append("circuit")
    if p03_wiki.get_card_quality(p03_wiki.get_card_data(card_name)) == "Rare":
        secondary.append("pkcm")
    if "scavenger" in card_name.lower():
        secondary.append("pkcm")
    return list(set(secondary))


def get_request_body(card_name):
    data = p03_wiki.get_card_data(card_name)
    return [
        "p03",
        {
            "name": data["displayedName"],
            "rare": p03_wiki.get_card_quality(data) == "Rare",
            "terrain": False,
            "health": data["baseHealth"],
            "useCustomPortrait": True,
            "portrait": {
                "imageBase64": "data:image/png;base64," + get_portrait_b64(card_name)
            },
            "portraitId": "",
            "useStatIcon": False,
            "attack": data["baseAttack"],
            "statIcon": "ants",
            "temple": "tech",
            "bloodCost": data["cost"],
            "boneCost": data["bonesCost"],
            "energyCost": data["energyCost"],
            "gemsCost": [],
            "abilities": get_abilities_request_body(data),
            "tribes": "$W1",
            "flags": "$W2",
            "evolvesInto": "",
            "evolvesIntoTurns": 1,
            "evolvesIntoName": "",
            "iceCubedInto": "",
            "description": "",
        },
        {"locale": "default", "border": False, "scanlines": True},
    ]


boundary = "---------------------------4172021632017129141242411954"


def generate_request_text(request, boundary, secondary_data=[]):
    second_string = ""
    if len(secondary_data) > 0:
        second_string = ",".join(f'"{s}"' for s in secondary_data)
    return (
        f"--{boundary}\r\n"
        'Content-Disposition: form-data; name="1"\r\n\r\n'
        "[]\r\n"
        f"--{boundary}\r\n"
        'Content-Disposition: form-data; name="2"\r\n\r\n'
        f"[{second_string}]\r\n"
        f"--{boundary}\r\n"
        'Content-Disposition: form-data; name="0"\r\n\r\n'
        f'{json.dumps(request, separators=(",", ":"))}\r\n'
        f"--{boundary}--\r\n"
    )


def cover_energy_cost(fname):
    # Guys, this code sucks I don't care
    card = Image.open(fname)
    mask = Image.open(os.path.join("cardexports", f"COMPONENT_alt_cost_background.png"))
    card.paste(mask, (431, 197))
    card.save(fname)


def add_blood_cost(fname, blood_cost, offset=0):
    # Guys, this code sucks I don't care
    if offset >= 5:
        return

    lul = "lit" if blood_cost > offset else "unlit"
    card = Image.open(fname)
    mask = Image.open(os.path.join("cardexports", f"COMPONENT_blood_cost_{lul}.png"))
    card.paste(mask, (626 - (offset * 47), 205), mask=mask)
    card.save(fname)

    add_blood_cost(fname, blood_cost, offset + 1)


def add_gems_cost(fname, gems_cost):
    mapping = {0: "green", 1: "orange", 2: "blue"}
    card = Image.open(fname)

    for offset, gidx in enumerate(gems_cost):
        mask = Image.open(
            os.path.join("cardexports", f"COMPONENT_{mapping[gidx]}_gem_cost.png")
        )
        card.paste(mask, (589 - (offset * 70), 205), mask=mask)

    card.save(fname)


def add_bones_cost(fname, bones_cost):
    card = Image.open(fname)
    mask_dig = Image.open(
        os.path.join("cardexports", f"Display_{bones_cost}_small.png")
    )
    mask_0 = Image.open(os.path.join("cardexports", f"Display_0_small.png"))
    mask_x = Image.open(os.path.join("cardexports", f"Display_x_small.png"))
    mask_b = Image.open(os.path.join("cardexports", f"BoneCostIcon_small.png"))
    card.paste(mask_dig, (625, 204), mask=mask_dig)
    card.paste(mask_0, (625 - 44, 204), mask=mask_0)
    card.paste(mask_x, (625 - (44 * 2), 204), mask=mask_x)
    card.paste(mask_b, (625 - (44 * 2) - 53, 204), mask=mask_b)
    card.save(fname)


def add_fuel_meter(fname, fuel):
    card = Image.open(fname)
    fuel_a = Image.open(os.path.join("cardexports", f"COMPONENT_fuel_gauge_{fuel}.png"))
    fuel_b = Image.open(os.path.join("cardexports", f"COMPONENT_fuel_gauge_{fuel}.png"))
    fuel_a.paste(card, (26, 0), mask=card)
    fuel_a.paste(fuel_b, (0, 0), mask=fuel_b)
    fuel_a.save(fname)


if __name__ == "__main__":

    prefix = "P03KCMXP3" if "--xp3" in sys.argv else None

    for fname in glob.glob(
        os.path.join(os.environ["INSCRYPTION_PATH"], "cardexports", "card*.json")
    ):

        card_name = os.path.basename(fname).replace("card_", "").replace(".json", "")
        if "!" in card_name:
            continue

        data = p03_wiki.get_card_data(card_name)

        if "scavenger_" not in card_name.lower():
            continue

        if prefix is not None and not card_name.startswith(prefix):
            continue

        if p03_wiki._special_enums()["CardTemple"][str(data["temple"])] != "Tech":
            continue

        try:
            second_data = get_secondary_data(card_name)

            request_body = get_request_body(card_name)
            text = generate_request_text(
                request_body, boundary, secondary_data=second_data
            )
            print(f"Requesting {card_name} from generator.cards")
            post_req = requests.post(
                "https://generator.cards/",
                verify=True,
                headers={
                    # "Accept": "text/x-component",
                    # "Accept-Encoding": "gzip, deflate, br",
                    "Content-Length": str(len(text)),
                    "Next-Action": "fa76829893dd61abb13da4be540131874f8a4365",
                    "Next-Router-State-Tree": "%5B%22%22%2C%7B%22children%22%3A%5B%22(front)%22%2C%7B%22children%22%3A%5B%22__PAGE__%22%2C%7B%7D%5D%7D%5D%7D%2Cnull%2Cnull%2Ctrue%5D",
                    "Next-Url": "/",
                    # "Connection": "keep-alive",
                    "Content-Type": f"multipart/form-data; boundary={boundary}",
                },
                data=text,
            )
            post_req.raise_for_status()
            b64img = post_req.content.decode().split("base64,")[1].split(":")[0]
            try:
                wraparound = (
                    post_req.content.decode().split(",data:image/png")[0].split("2:")[1]
                )
            except Exception:
                wraparound = ""
            filename = os.path.join("cardexports", f"{card_name}.png")
            with open(filename, "wb") as f:
                try:
                    f.write(base64.b64decode(b64img))
                except Exception:
                    f.write(base64.b64decode(b64img + wraparound))

            # Add the blood cost decal
            if data["cost"] > 0:
                cover_energy_cost(filename)
                add_blood_cost(filename, data["cost"])

            if len(data["gemsCost"]) > 0:
                cover_energy_cost(filename)
                add_gems_cost(filename, data["gemsCost"])

            if data["bonesCost"] > 0:
                cover_energy_cost(filename)
                add_bones_cost(filename, data["bonesCost"])

            if data["startingFuel"] > 0:
                add_fuel_meter(filename, data["startingFuel"])

        except Exception as ex:
            print(f"Failed to generate {card_name}: {ex}")
