import requests
import p03_wiki
import os
import json
import base64
import glob
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
    return secondary


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
            "statIcon": "",
            "temple": "tech",
            "bloodCost": 0,
            "boneCost": 0,
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


if __name__ == "__main__":
    for fname in glob.glob(
        os.path.join(os.environ["INSCRYPTION_PATH"], "cardexports", "card*.json")
    ):

        card_name = os.path.basename(fname).replace("card_", "").replace(".json", "")
        if "!" in card_name:
            continue

        data = p03_wiki.get_card_data(card_name)
        if p03_wiki._special_enums()["CardTemple"][str(data["temple"])] != "Tech":
            continue

        try:
            second_data = get_secondary_data(card_name)
            # Temporary for this rebuild:
            if "pkcm" not in second_data:
                continue
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
                    "Next-Action": "ee0c6925fd7cec9fab62399a6430fb0316fac4a2",
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
            with open(os.path.join("cardexports", f"{card_name}.png"), "wb") as f:
                try:
                    f.write(base64.b64decode(b64img))
                except Exception:
                    f.write(base64.b64decode(b64img + wraparound))
        except Exception as ex:
            print(f"Failed to generate {card_name}: {ex}")
