import mwclient
from mwclient.page import Page
import json
import os
import functools
import glob
import time
from dotenv import load_dotenv
load_dotenv()

# Some basic error handling
if "INSCRYPTION_PATH" not in os.environ:
    raise RuntimeError("Environment variable `INSCRYPTION_PATH` not set to the location of Inscryption")

if not os.path.exists(os.path.join(os.environ["INSCRYPTION_PATH"], "cardexports")):
    raise RuntimeError("No metadata found in Inscryption game directory. Run P03 in Kaycee's Mod, go to the main map screen in-game, and enter the CTRL+SHIFT+E key combination. Wait for it to finish, close the game, then come back here.")

def login() -> mwclient.Site:
    site = mwclient.Site('p03kcmod.wiki.gg', httpauth=(os.environ["PYWIKI_DEV_USER"], os.environ["PIWIKI_DEV_PASSWORD"]), path="/")
    site.login(os.environ["PYWIKI_USER"], os.environ["PYWIKI_PASSWORD"])
    return site

@functools.cache
def _special_enums():
    with open(os.path.join(os.environ["INSCRYPTION_PATH"], "cardexports", "special_enums.json")) as f:
        return json.load(f)
    
# Build the ability dictionary
@functools.cache
def get_ability_dictionary():
    retval = {}
    for fname in glob.glob(os.path.join(os.environ["INSCRYPTION_PATH"], "cardexports", "ability*.json")):
        name = os.path.basename(fname).replace('ability_','').replace('.json', '')
        if name is None or len(name) == 0:
            continue
        with open(fname) as f:
            ability = json.load(f)
        ability['name'] = name
        retval[ability['ability']] = ability
    return retval

@functools.cache
def get_card_dictionary():
    retval = {}
    for fname in glob.glob(os.path.join(os.environ["INSCRYPTION_PATH"], "cardexports", "card*.json")):
        name = os.path.basename(fname).replace('card_','').replace('.json', '')
        if name is None or len(name) == 0:
            continue
        with open(fname) as f:
            ability = json.load(f)
        ability['name'] = name
        retval[name] = ability
    return retval

@functools.cache
def get_card_data(card_name):
    with open(os.path.join(os.environ["INSCRYPTION_PATH"], 'cardexports', f'card_{card_name}.json')) as f:
        return json.load(f)
    
def card_has_ability(data, ability: str):
    ability_dict = get_ability_dictionary()
    ability = ability.lower()
    for ab in data['abilities']:
        try:
            if ability == ability_dict[int(ab)]['name'].lower():
                return True
            if ability == ability_dict[int(ab)]['rulebookName'].lower():
                return True
        except Exception:
            continue
    return False


def get_card_quality(data):
    metas = _special_enums()['CardMetaCategory']
    is_rare = False
    is_common = False
    for mcat in data['metaCategories']:
        if metas[str(mcat)] == 'Rare':
            is_rare = True
        if metas[str(mcat)] == 'ChoiceNode':
            is_common = True
    if is_rare:
        return 'Rare'
    if is_common:
        return 'Common'
    return 'Unobtainable'

def get_card_region(data):
    metas = _special_enums()['CardMetaCategory']
    results = []
    for mcat in data['metaCategories']:
        if metas[str(mcat)].endswith('NeutralRegionCards'):
            results.append('Neutral')
        if metas[str(mcat)].endswith('WizardRegionCards'):
            results.append('Magick')
        if metas[str(mcat)].endswith('TechRegionCards'):
            results.append('Tech')
        if metas[str(mcat)].endswith('NatureRegionCards'):
            results.append('Nature')
        if metas[str(mcat)].endswith('UndeadRegionCards'):
            results.append('Undead')
    if len(results) > 0:
        return ", ".join(results)
    return 'Unobtainable'

def get_extra_card_properties(cardname, index):
    index = "" if index is None or index == 1 else f"_{index}"

    data = get_card_data(cardname)
    ability_dict = get_ability_dictionary()

    expansion_pack = ("Expansion Pack #2" if cardname.startswith('P03KCMXP2')
                    else "Expansion Pack #1" if cardname.startswith('P03KCMXP1')
                    else "P03 in Kaycee's Mod" if cardname.startswith('P03KCM')
                    else "Base Game")
    
    cost = f"{data['energyCost']} Energy"

    abilityString = "{{Pagelist|" + "|".join(ability_dict[ab]['rulebookName'] for ab in data['abilities']) + "}}"

    quality = get_card_quality(data)
    region = get_card_region(data)

    if quality == 'Unobtainable' and region != 'Unobtainable':
        quality = 'Common'

    # Start generating the page data
    return (
        f"| id{index} = {cardname}\n"
        f"| artist{index} = \n"
        f"| expansion{index} = {expansion_pack}\n"
        f"| cost{index} = {cost}\n"
        f"| attack{index} = {data['baseAttack']}\n"
        f"| health{index} = {data['baseHealth']}\n"
        f"| sigils{index} = {abilityString}\n"
        f"| quality{index} = {quality}\n"
        f"| region{index} = {region}\n"
    )

def generate_ability_page(ability, raise_errors = False):

    try:
        ability_dict = get_ability_dictionary()
        data = [d for d in ability_dict.values() if d['name'] == ability][0]

        sigil = "Yes" if 3 in data['metaCategories'] else "No"

        modifiers = "a"
        typestr = "Trigger"
        if data['activated']:
            modifiers = "an activated"
            typestr = "Activated"
        elif data['passive']:
            modifiers = "a passive"
            typestr = "Passive"

        desc = data['rulebookDescription'].replace("[creature]", "a card bearing this sigil")
        desc = desc[0].upper() + desc[1:]

        retval = (
            "{{AbilityBox\n"
            f"| rulebook_name = {data['rulebookName']}\n"
            f"| type = {typestr}\n"
            f"| id = {ability}\n"
            f"| power_level = {data['powerLevel']}\n"
            f"| sigil_machine = {sigil}\n"
            "}}\n\n"
            "{{Quote|" + desc + "}}\n\n"
            "== Summary ==\n\n"
            f"{data['rulebookName']} is {modifiers} sigil "
        )

        cards = [c['displayedName'] for c in get_card_dictionary().values() if '!' not in c['displayedName'] and _special_enums()['CardTemple'][str(c['temple'])] == 'Tech' and 'abilities' in c and data['ability'] in c['abilities']]
        cards = list(set(cards))
        

        if len(cards) == 0:
            retval += "that does not appear on any cards in P03 in Kaycee's Mod.\n\n"

        elif len(cards) == 1:
            retval += f"that appears on the card [[{cards[0]}]].\n\n"

        elif len(cards) > 5:
            retval += f"that appears on {len(cards)} cards.\n\n"

        else:
            retval += "that appears on the cards {{Pagelist|"
            retval += "|".join(cards)
            retval += "}}\n\n"

        retval += "== Trivia ==\n\nThis section is blank.\n\n[[Category:Sigils]]"

        return len(cards), retval

    except Exception as ex:
        if raise_errors:
            raise
        print(f"Could not generate page for {ability}: {ex}")
        return None, None
    
def generate_card_page(cardname, raise_errors = False):
    
    if "!" in cardname:
        return None, None, None
    
    data = get_card_data(cardname)

    # Must be a tech temple card
    if _special_enums()['CardTemple'][str(data['temple'])] != 'Tech':
        return None, None, None

    try:
        expansion_pack = ("Expansion Pack #2" if cardname.startswith('P03KCMXP2')
                        else "Expansion Pack #1" if cardname.startswith('P03KCMXP1')
                        else "P03 in Kaycee's Mod" if cardname.startswith('P03KCM')
                        else "Base Game")
        
        all_card_datas = {"Base": get_extra_card_properties(cardname, 1)}

        is_obtainable = get_card_region(data) != 'Unobtainable'
        is_root = False
        
        if "evolveParams" in data:
            if card_has_ability(data, "fledgling"):
                is_root = is_obtainable
                all_card_datas |= {"Evolution": get_extra_card_properties(data['evolveParams']['evolution'], len(all_card_datas) + 1)}
            elif card_has_ability(data, "transformer") or card_has_ability(data, "transforms when powered") or card_has_ability(data, "transforms when unpowered"):
                is_root = is_obtainable
                all_card_datas |= {"Transform": get_extra_card_properties(data['evolveParams']['evolution'], len(all_card_datas) + 1)}

        if "iceCubeParams" in data:
            if card_has_ability(data, "frozen away"):
                is_root = is_obtainable
                all_card_datas |= {"Dies Into": get_extra_card_properties(data['iceCubeParams']['creatureWithin'], len(all_card_datas) + 1)}

        # Start generating the page data
        if len(all_card_datas) == 1:
            retval = (
                "{{CardBox\n"
                f"| name = {data['displayedName']}\n"
            )
            retval += all_card_datas['Base']
        else:
            retval = (
                "{{TabbedCardBox\n"
                f"| name = {data['displayedName']}\n"
            )
            for idx, (tab, contents) in enumerate(all_card_datas.items()):
                retval += f"| tab_{idx+1} = {tab}\n"
                retval += contents
        retval += "}}\n"

        retval += f"\n== Summary ==\n\n{data['displayedName']} is a card from "
        if expansion_pack.startswith('Expansion'):
            retval += f"[[{expansion_pack.replace('#', '')}]]. "
        elif "P03" in expansion_pack:
            retval += f"the core release of P03 in Kaycee's Mod. "
        else:
            retval += f"the vanilla game. "
        
        retval += "\n\n== Strategy ==\n\nThis section is currently blank.\n\n== Trivia ==\n\nThis section is currently blank.\n\n[[Category:Cards]]"

        return data['displayedName'], retval, is_root
    except Exception as ex:
        if raise_errors:
            raise
        print(f'Failed to generate page for {cardname}: {ex}')
        return None, None, None
    
def edit_page_contents(site: mwclient.Site, page_name, contents, comments="Initial Setup", retries=5, overwrite_if_exists=False):
    page: Page = site.pages[page_name]
    if not page.exists or overwrite_if_exists:
        print(f"Creating page {page_name}")
        for retry in range(retries):
            try:
                page.edit(contents, comments)
            except Exception as ex:
                print(f"Failed attempt {retry} for {page_name}: {ex}")
                time.sleep(10)
    else:
        print(f"Not creating page {page_name} because it already exists")
    
def publish_card_pages(overwrite_if_exists=False):
    generated_pages = {}
    client = login()
    for fname in glob.glob(os.path.join(os.environ["INSCRYPTION_PATH"], 'cardexports', "card*.json")):

        cname = os.path.basename(fname).replace('card_', '').replace('.json', '')

        page_name, page_contents, is_root = generate_card_page(cname)
        if page_contents != None:
            if page_name not in generated_pages:
                generated_pages[page_name] = []
            
            generated_pages[page_name].append((is_root, page_contents))

    for key, pages in generated_pages.items():
        page = pages[0][1]
        if len(pages) > 1:
            print(f'{key} has {len(pages)} pages')
            for i, p in enumerate(pages):
                if p[0]:
                    page = pages[i][1]

        edit_page_contents(client, key, page, overwrite_if_exists=overwrite_if_exists)

def publish_ability_pages(overwrite_if_exists=False):
    client = login()
    for fname in glob.glob(os.path.join(os.environ["INSCRYPTION_PATH"], 'cardexports', "ability*.json")):

        aname = os.path.basename(fname).replace('ability_', '').replace('.json', '')

        number_of_cards, page_contents = generate_ability_page(aname)
        if page_contents != None and number_of_cards > 0:
            edit_page_contents(client, aname, page_contents, overwrite_if_exists=overwrite_if_exists)

def touch_all_category(category_name: str):
    client = login()
    for page in client.categories[category_name]:
        try:
            print(f"Touching {page.name}")
            page.touch()
        except:
            print("Taking a break")
            time.sleep(30)
            page.touch()

def update_card_categories(category_name: str):
    """Updates the card category summary for a given category"""
    client = login()

    if category_name == "Unobtainable Cards":
        unob_cards = []
    else:
        unob_cards = list(p.name for p in client.categories["Unobtainable Cards"])

    outtext = '<gallery mode="nolines">\n'
    #xp2_cards = list(p for p in site.categories["Base Game Cards"] if p.name not in unob_cards)
    for p in client.categories[category_name]:
        if p.name in unob_cards:
            continue
        text = p.text()
        fname = [l.replace('| id = ', '') for l in text.splitlines() if "id = " in l]
        if len(fname) == 0:
            continue
        fname = fname[0]
        outtext += f'File:{fname}.png|[[{p.name}]]\n'
    outtext += '</gallery>'

    page = client.pages[f"Category:{category_name}"]
    page.edit(outtext)