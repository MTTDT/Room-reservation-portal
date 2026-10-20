export const EQUIPMENT = ["Projector", "TvScreen", "VideoConferencing", "Whiteboard"] as const;

export type Equipment = (typeof EQUIPMENT)[number];

export const EQUIPMENT_LABELS: Record<Equipment, string> = {
    Projector: "Projector",
    TvScreen: "TV screen",
    VideoConferencing: "Video conferencing",
    Whiteboard: "Whiteboard",
};

