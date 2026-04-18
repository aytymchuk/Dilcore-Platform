window.dilcoreAgent = {
    scrollContainerToEnd: function (el) {
        if (!el) {
            return;
        }
        el.scrollTop = el.scrollHeight;
    },
    copyText: async function (text) {
        if (!text) {
            return;
        }
        await navigator.clipboard.writeText(text);
    }
};
