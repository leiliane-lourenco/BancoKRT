// Suporte a valores monetários no padrão brasileiro (vírgula como separador decimal).
// Aplica-se aos <input class="money"> e faz a validação client-side aceitar "0,00".
(function () {
    "use strict";

    // Converte um texto pt-BR ("1.000,00") em número; retorna null se vazio/ inválido.
    function paraNumero(valor) {
        if (valor === null || valor === undefined) return null;
        var texto = String(valor).trim();
        if (texto === "") return null;
        // Remove separador de milhar (.) e troca a vírgula decimal por ponto.
        var n = parseFloat(texto.replace(/\./g, "").replace(",", "."));
        return isNaN(n) ? null : n;
    }

    // Formata o campo como "0,00" (duas casas, padrão pt-BR).
    function formatar(el) {
        var n = paraNumero(el.value);
        el.value = n === null ? "" : n.toLocaleString("pt-BR", {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    function aplicar(el) {
        formatar(el); // normaliza o valor inicial (ex.: "1000" -> "1.000,00")
        el.addEventListener("blur", function () { formatar(el); });
    }

    document.querySelectorAll("input.money").forEach(aplicar);

    // Faz o jQuery Validate (validação não intrusiva) entender o formato pt-BR,
    // caso contrário "1.000,00" seria rejeitado no cliente antes de enviar.
    if (window.jQuery && window.jQuery.validator) {
        window.jQuery.validator.methods.number = function (value, element) {
            return this.optional(element)
                || /^-?\d{1,3}(\.\d{3})*(,\d+)?$/.test(value)
                || /^-?\d+(,\d+)?$/.test(value);
        };
        window.jQuery.validator.methods.range = function (value, element, param) {
            var n = paraNumero(value);
            return this.optional(element) || (n !== null && n >= param[0] && n <= param[1]);
        };
    }
})();
