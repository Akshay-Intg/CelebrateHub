// ============================================================
// party-voting.js — Fixed version
// Fix 1: Button stuck on "Voting..." — re-enable buttons after
//         updateCard() since the DOM elements are reused.
// ============================================================

(function ($) {
    'use strict';

    // Read the anti-forgery token once
    var antiForge = $('input[name="__RequestVerificationToken"]').first().val();

    // ── Toast helper ──────────────────────────────────────────────────────────

    function showToast(message, isSuccess) {
        var toastEl = document.getElementById('voteToast');
        var toastMsg = document.getElementById('toastMsg');
        if (!toastEl || !toastMsg) return;

        toastMsg.textContent = message;
        toastEl.classList.remove('bg-success', 'bg-danger', 'text-white');
        toastEl.classList.add(isSuccess ? 'bg-success' : 'bg-danger', 'text-white');

        bootstrap.Toast.getOrCreateInstance(toastEl, { delay: 3500 }).show();
    }
    function restoreButtons(btn, voteType) {
        // Re-enable and restore the label of the button that was clicked
        btn.prop('disabled', false);
        var isDone = voteType === 'Done';
        btn.html(
            '<i class="bi ' + (isDone ? 'bi-check-circle' : 'bi-clock') + ' me-1"></i>' +
            (isDone ? 'Party Done' : 'Still Pending')
        );
    }

    // ── Progress bar colour ───────────────────────────────────────────────────

    function getBarClass(pct) {
        return pct >= 75 ? 'bg-success' : pct >= 50 ? 'bg-warning' : 'bg-danger';
    }

    // ── Live card update after a successful vote ───────────────────────────────

    function updateCard(eventId, updatedEvent) {
        if (!updatedEvent) return;

        var card = $('#event-card-' + eventId + ' .party-event-card');
        if (!card.length) return;

        var pct = parseFloat(updatedEvent.donePercentage).toFixed(0);
        var pending = (100 - parseFloat(updatedEvent.donePercentage)).toFixed(0);

        // Progress bar
        var barClass = getBarClass(parseFloat(pct));
        card.find('.progress-bar')
            .css('width', pct + '%')
            .attr('aria-valuenow', pct)
            .removeClass('bg-success bg-warning bg-danger')
            .addClass(barClass);

        // Percentage labels
        card.find('.party-progress-labels')
            .html('<span class="text-success fw-semibold">✅ ' + pct + '% Done</span>' +
                '<span class="text-danger fw-semibold">❌ ' + pending + '% Pending</span>');

        // Vote counts
        card.find('.vote-count-item.vote-done .vote-num').text(updatedEvent.doneVotes);
        card.find('.vote-count-item.vote-pending .vote-num').text(updatedEvent.pendingVotes);

        // Total votes text (sits right after vote-counts div)
        card.find('.small.text-muted').filter(function () {
            return $(this).text().indexOf('vote') !== -1;
        }).text(updatedEvent.totalVotes + ' vote' + (updatedEvent.totalVotes !== 1 ? 's' : ''));

        // Status badge
        var statusLabel = updatedEvent.isPartyGiven ? 'Party Given ✅' : 'Party Pending ❌';
        var statusClass = updatedEvent.isPartyGiven ? 'party-status-given' : 'party-status-pending';
        card.find('.party-status-badge')
            .text(statusLabel)
            .removeClass('party-status-given party-status-pending')
            .addClass(statusClass);

        // Card border for "Party Given"
        card.removeClass('border-success');
        if (updatedEvent.isPartyGiven) card.addClass('border-success');

        // ── FIX: Re-enable ALL vote buttons first, then update their labels ──
        var myVote = updatedEvent.myVote;

        card.find('.btn-vote').each(function () {
            var btn = $(this);
            var voteType = btn.data('vote-type');
            var isDone = voteType === 'Done';
            var isChosen = voteType === myVote;

            // Always re-enable — this was the bug (buttons stayed disabled)
            btn.prop('disabled', false);

            // Update already-voted data attribute for next click
            btn.data('already-voted', myVote ? 'true' : 'false');

            // Reset active classes
            btn.removeClass('btn-vote-active btn-vote-active-pending');

            if (isChosen) {
                // This is the button the user just clicked — mark as selected
                btn.addClass(isDone ? 'btn-vote-active' : 'btn-vote-active-pending');
                btn.html('<i class="bi ' + (isDone ? 'bi-check-circle' : 'bi-clock') + ' me-1"></i>' +
                    '✔ You voted ' + voteType);
            } else {
                // The other button — restore its default label
                btn.html('<i class="bi ' + (isDone ? 'bi-check-circle' : 'bi-clock') + ' me-1"></i>' +
                    (isDone ? 'Party Done' : 'Still Pending'));
            }
        });

        // Brief pop animation
        card.addClass('card-updated');
        setTimeout(function () { card.removeClass('card-updated'); }, 600);
    }

    // ── Vote button click ─────────────────────────────────────────────────────

    $(document).on('click', '.btn-vote', function () {
        var btn = $(this);
        var eventId = btn.data('event-id');
        var voteType = btn.data('vote-type');
        var alreadyVoted = btn.data('already-voted') === true
            || btn.data('already-voted') === 'true';
        var isCurrentlyActive = btn.hasClass('btn-vote-active') ||
            btn.hasClass('btn-vote-active-pending');
        if (alreadyVoted && isCurrentlyActive) {
            showToast('You already voted ' + voteType + ' for this event.', false);
            return;
        }
        // Show spinner on clicked button only
        btn.prop('disabled', true);
        btn.html('<span class="spinner-border spinner-border-sm me-1"></span>Voting…');

        var url = alreadyVoted ? '/Party/ChangeVote' : '/Party/Vote';
        var payload = alreadyVoted
            ? { partyEventId: eventId, newVoteType: voteType }
            : { partyEventId: eventId, voteType: voteType };

        $.ajax({
            url: url,
            type: 'POST',
            data: Object.assign({}, payload, { __RequestVerificationToken: antiForge }),
            success: function (resp) {
                if (resp.success) {
                    showToast(
                        'Vote recorded!' + (resp.updatedEvent && resp.updatedEvent.isPartyGiven
                            ? ' 🎉 Party is now marked as Given!' : ''),
                        true
                    );
                    // updateCard re-enables buttons internally
                    updateCard(eventId, resp.updatedEvent);
                } else {
                    showToast(resp.message || 'Vote failed.', false);
                    // Re-enable on failure — restore original label
                    //btn.prop('disabled', false);
                    //var isDone = voteType === 'Done';
                    //btn.html('<i class="bi ' + (isDone ? 'bi-check-circle' : 'bi-clock') + ' me-1"></i>' +
                    //    (isDone ? 'Party Done' : 'Still Pending'));
                    restoreButtons(btn, voteType);
                }
            },
            error: function (xhr) {
                var msg = 'Something went wrong. Please try again.';
                try {
                    var json = JSON.parse(xhr.responseText);
                    if (json.message) {
                        msg = json.message;
                    }
                    else if (json.data && json.data.message)
                        msg = json.data.message;
                } catch (e) { }
                showToast(msg, false);
                // Re-enable on error
                //btn.prop('disabled', false);
                //var isDone = voteType === 'Done';
                //btn.html('<i class="bi ' + (isDone ? 'bi-check-circle' : 'bi-clock') + ' me-1"></i>' +
                //    (isDone ? 'Party Done' : 'Still Pending'));
                restoreButtons(btn, voteType);
            }
        });
    });

})(jQuery);