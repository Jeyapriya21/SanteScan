<template>
	<section class="bg-white border border-slate-200 rounded-2xl shadow-sm p-6">
		<header class="flex items-center justify-between mb-4">
			<h2 class="text-lg font-black tracking-tight text-slate-800">
				Historique recent
			</h2>
			<span class="text-xs uppercase tracking-[0.2em] text-slate-400">Derniers scans</span>
		</header>

		<ul class="divide-y divide-slate-100">
			<li class="py-3 flex items-start justify-between gap-4">
				<div>
					<p class="text-sm font-bold text-slate-800">Bilan sanguin</p>
					<p class="text-xs text-slate-500">12 fev 2026 - 09:42</p>
				</div>
				<span class="text-xs font-bold text-emerald-700 bg-emerald-50 px-2 py-1 rounded-full">OK</span>
			</li>
			<li class="py-3 flex items-start justify-between gap-4">
				<div>
					<p class="text-sm font-bold text-slate-800">Analyse cholesterol</p>
					<p class="text-xs text-slate-500">08 fev 2026 - 17:10</p>
				</div>
				<span class="text-xs font-bold text-amber-700 bg-amber-50 px-2 py-1 rounded-full">A surveiller</span>
			</li>
			<li class="py-3 flex items-start justify-between gap-4">
				<div>
					<p class="text-sm font-bold text-slate-800">NFS</p>
					<p class="text-xs text-slate-500">02 fev 2026 - 08:03</p>
				</div>
				<span class="text-xs font-bold text-emerald-700 bg-emerald-50 px-2 py-1 rounded-full">OK</span>
			</li>
		</ul>
	</section>
</template>
