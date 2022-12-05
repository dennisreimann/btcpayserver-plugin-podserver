"use strict"

document.addEventListener("DOMContentLoaded", () => {
    const { player } = window.PodServer
    
    // Player
    if (window.Amplitude && player) {
        window.Amplitude.init(player)

        document.querySelector('.player__progress').addEventListener('click', function (e) {
            const offset = this.getBoundingClientRect()
            const x = e.pageX - offset.left
            window.Amplitude.setSongPlayedPercentage((parseFloat(x) / parseFloat(this.offsetWidth)) * 100)
        })
    }
})
