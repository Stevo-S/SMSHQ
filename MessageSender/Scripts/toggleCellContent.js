$(function () {
    // Toggle wrap-around on ellipsed text in tables on some views
    $('tr td:first-child').click(function () {
        var newValue = $(this).css('white-space') === 'nowrap' ? 'normal' : 'nowrap';
        $(this).css('white-space', newValue);

        // Toggle mouse hand
        $(this).toggleClass('handHover');
    });

}
);