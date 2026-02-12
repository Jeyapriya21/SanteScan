<template>
  <div id="app" class="min-h-screen bg-slate-50">
    
    <HomeView />

  </div>
</template>

<script setup>
// On importe la page principale qui assemble tes composants
import HomeView from './assets/views/HomeView.vue'
</script>

<style>
/* Ici, on s'assure que le corps de la page n'a pas de marges 
  et que la police par défaut est propre.
*/
body {
  margin: 0;
  padding: 0;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}
</style>