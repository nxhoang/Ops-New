var chartColors = {
    red: '#ff6384',
    orange: '#ff9f40',
    yellow: '#ffcd56',
    green: '#4bc0c0',
    blue: '#36a2eb',
    gray1: '#5B6470',
    //gray2: '#8C94A1',
    //gray3: '#BDC7D6',
    gray2: '#818b98',
    gray3: '#abb1ba',
    gray4: '#d5d8dd',
    gray5: '#1a1a00'

};

function convertColorHexToRgbOpacity(hex, opacity) {
    hex = hex.replace('#', '');
    r = parseInt(hex.substring(0, 2), 16);
    g = parseInt(hex.substring(2, 4), 16);
    b = parseInt(hex.substring(4, 6), 16);
    result = 'rgba(' + r + ',' + g + ',' + b + ',' + opacity + ')';

    return result;
}