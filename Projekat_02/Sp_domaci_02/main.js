// let tekst = document.querySelector(`.inputPolje`);
// let dugme = document.querySelector(`.buttonPolje`);
// dugme.onclick = ()=>{
//     tekst.innerHTML = "kliknuto je";
// };

document.addEventListener("DOMContentLoaded", function() {
    const textInput = document.querySelector(".inputPolje");
    const changeButton = document.querySelector(".buttonPolje");
    const outputLabel1 = document.querySelector(".labelPolje1");
    const outputLabel2 = document.querySelector(".labelPolje2");

    changeButton.addEventListener("click", async () => {
        const inputValue = textInput.value;
        //outputLabel.textContent = inputValue;
        const res = await fetch(`http://localhost:5050/${inputValue}.txt`).then(
            res => res.text()
        ).then(
            text => {
                let label_text = text.split('/');
                
                outputLabel1.innerHTML = label_text[0];
                outputLabel2.innerHTML = label_text[1];
            }
        );

    });
});

