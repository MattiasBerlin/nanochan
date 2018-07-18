// TODO Make parameter textBlock instead? function even needed?
function addText(text) {
    content = document.getElementById('content');
    textParagraph = document.createElement('span');
    textParagraph.innerHTML = text;
    content.appendChild(document.createElement('br'));
    content.appendChild(document.createElement('br'));
    content.appendChild(textParagraph);
}

var textBlock; // TODO need? NO

function appendWord(id, word, hasDefinition, reading) {
    alert("id: " + id);
    alert("word: " + word);
    alert("hasDefinition: " + hasDefinition);
    alert("reading: " + reading);

    if (word == null) {
        return;
    }
    if (!textBlock) {
        textBlock = document.createElement('span');
    }
    
    if (hasDefinition === true) {
        alert("true :D");
    }
}

function appendName(name, reading) {

}

function commitBlock(id) {

}

function deleteBlock(id) {
    alert("yup");
}

// TODO Make a try catch and show a fancy error if it fails
function addBlock(blockID, wordsJSON) {
    words = JSON.parse(wordsJSON);

    if (words.length > 0) {
        content = document.getElementById('content');
        textParagraph = document.createElement('span');

        for (i = 0; i < words.length; ++i) {
            let word = words[i];
            if (word.hasDefinition === true) {
                let wordSpan = document.createElement('span');
                wordSpan.className = "word";

                if (word.furigana) {
                    ruby = document.createElement('ruby');
                    cutOff = word.furigana.length;
                    ruby.innerHTML = word.word.substr(0, word.furiganaLength);

                    rt = document.createElement('rt');
                    rt.innerHTML = word.furigana;
                    ruby.appendChild(rt);
                    ruby.innerHTML += word.word.substr(cutOff);

                    wordSpan.appendChild(ruby);
                } else {
                    wordSpan.innerHTML = word.word;
                }

                // Add event listeners
                wordSpan.addEventListener('mouseover', (function (blockID, wordID, word) {
                    return function () {
                        onWordHover(blockID, wordID, word);
                    }
                })(blockID, words[i].id, wordSpan));
                wordSpan.addEventListener('mouseout', (function () {
                    return function () {
                        onWordStopHover();
                    }
                })());

                textParagraph.appendChild(wordSpan);
            } else { // TODO Add check if \n then add br element
                t = document.createTextNode(words[i].word);
                textParagraph.appendChild(t);
            }
        }
        content.appendChild(textParagraph);
        content.appendChild(document.createElement('br'));
        content.appendChild(document.createElement('br'));
        // TODO Maybe add more br to make the new block the only visible text?
        // TODO Go to top of new block
        window.scrollTo(0, document.body.scrollHeight);
    }
    // <span class="word"><ruby>漢字<rt>かんじ</rt></ruby></span>
}

function addWordEventListeners() {
    //console.log("Adding word event listeners");

    var elements = document.querySelectorAll('.word,.name');
    for (i = 0; i < elements.length; ++i) {
        el = elements[i];
        //console.log(elements[i].innerHTML);
        if (el.addEventListener) {
            //console.log(elements[i].innerHTML + " passed");
            if (el.childElementCount > 0) {
                word = el.firstChild.firstChild.data;
            } else {
                word = el.innerHTML;
            }
            el.addEventListener('mouseover', (function (word) {
                return function() {
                    onWordHover(word);
                }
            })(word));
            //elements[i].addEventListener('mouseleave', onWordStopHover);
        }
    }
}

function onWordHover(blockID, wordID, word) {
    let wordRect = word.getBoundingClientRect();
    window.external.OnWordHover(blockID, wordID, wordRect.left, wordRect.bottom);
    //window.external.OnWordHover(word.offsetLeft - word.scrollLeft + word.clientLeft, word.offsetTop - word.scrollTop + word.clientTop, word.innerHTML);
    //alert(word);
}

function onWordStopHover() {
    window.external.OnWordStopHover();
}