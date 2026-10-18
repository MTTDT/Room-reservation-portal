import * as React from 'react';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import ButtonBase from '@mui/material/ButtonBase';
import IconButton from '@mui/material/IconButton';
import Paper from '@mui/material/Paper';
import ToggleButton from '@mui/material/ToggleButton';
import ToggleButtonGroup from '@mui/material/ToggleButtonGroup';
import Tooltip from '@mui/material/Tooltip';
import Typography from '@mui/material/Typography';
import { alpha, type Theme } from '@mui/material/styles';
import ChevronLeftIcon from '@mui/icons-material/ChevronLeft';
import ChevronRightIcon from '@mui/icons-material/ChevronRight';
import type { Room } from '../types/room'
import type { Reservation, UpdateReservation } from '../types/reservation';
import { updateReservation } from '../api/reservationApi';
import { ReservationStatus, reservationStatusLabels } from '../types/reservationStatus';

/* ------------------------------------------------------------------ */
/* Types                                                               */
/* ------------------------------------------------------------------ */

export type DateInput = Date | string | number;


export type TimelineView = 'day' | 'week' | 'month';

export interface Slot {
  roomId: number;
  start: Date;
}

export interface RoomTimelineProps {
  showOnlyMy: boolean,
  rooms: Room[];
  reservations: Reservation[];
  userId: number | null;
  defaultView?: TimelineView;
  defaultDate?: DateInput;
  weekStartsOn?: 0 | 1;
  locale?: string;
  onBookingClick?: (updateReq: UpdateReservation, resId: number, uId: number) => void;
  onSlotClick?: (slot: Slot) => void;
  className?: string;
  style?: React.CSSProperties;
}

/* ------------------------------------------------------------------ */
/* Date helpers                                                        */
/* ------------------------------------------------------------------ */

function toDate(v: DateInput): Date {
  if (v instanceof Date) return new Date(v.getTime());
  if (typeof v === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(v)) {
    const [y, m, d] = v.split('-').map(Number);
    return new Date(y, m - 1, d);
  }
  return new Date(v);
}

const startOfDay = (d: Date) => new Date(d.getFullYear(), d.getMonth(), d.getDate());
const addDays = (d: Date, n: number) => new Date(d.getFullYear(), d.getMonth(), d.getDate() + n);

interface Column {
  start: number;
  end: number;
  date: Date;
}

function getRange(view: TimelineView, anchor: Date, weekStartsOn: 0 | 1) {
  const day = startOfDay(anchor);
  if (view === 'day') return { start: day, end: addDays(day, 1) };
  if (view === 'week') {
    const diff = (day.getDay() - weekStartsOn + 7) % 7;
    const start = addDays(day, -diff);
    return { start, end: addDays(start, 7) };
  }
  const start = new Date(day.getFullYear(), day.getMonth(), 1);
  return { start, end: new Date(start.getFullYear(), start.getMonth() + 1, 1) };
}

function buildColumns(view: TimelineView, range: { start: Date; end: Date }): Column[] {
  const cols: Column[] = [];
  if (view === 'day') {
    const y = range.start.getFullYear();
    const m = range.start.getMonth();
    const d = range.start.getDate();
    for (let h = 0; h < 24; h++) {
      const s = new Date(y, m, d, h);
      const e = new Date(y, m, d, h + 1);
      cols.push({ start: s.getTime(), end: e.getTime(), date: s });
    }
    return cols;
  }
  for (let d = range.start; d < range.end; d = addDays(d, 1)) {
    cols.push({ start: d.getTime(), end: addDays(d, 1).getTime(), date: d });
  }
  return cols;
}

function colPos(cols: Column[], t: number): number {
  if (t <= cols[0].start) return 0;
  for (let i = 0; i < cols.length; i++) {
    if (t < cols[i].end) return i + (t - cols[i].start) / (cols[i].end - cols[i].start);
  }
  return cols.length;
}



const ROOM_W = 150;
const LANE_H = 34;
const BOOKING_H = 26;

const subtle = (t: Theme, opacity: number) =>
  alpha(t.palette.mode === 'dark' ? t.palette.common.white : t.palette.common.black, opacity);

const weekendBg = (t: Theme) => subtle(t, 0.02);
const hoverBg = (t: Theme) => subtle(t, 0.06);
const todayBg = (t: Theme) => alpha(t.palette.primary.main, 0.08);



export function RoomTimeline(props: RoomTimelineProps) {
  const {
    showOnlyMy,
    rooms,
    reservations,
    defaultView = 'week',
    defaultDate,
    weekStartsOn = 1,
    locale,
    onBookingClick,
    onSlotClick,
    className,
    style,

  } = props;

  const [view, setView] = React.useState<TimelineView>(defaultView);
  const [anchor, setAnchor] = React.useState<Date>(() =>
    startOfDay(defaultDate ? toDate(defaultDate) : new Date()),
  );

  const range = React.useMemo(() => getRange(view, anchor, weekStartsOn), [view, anchor, weekStartsOn]);
  const cols = React.useMemo(() => buildColumns(view, range), [view, range]);

  const normalized = React.useMemo(
    () => reservations.map((b) => ({ b, s: toDate(b.startTime).getTime(), e: toDate(b.endTime).getTime() })),
    [reservations],
  );

  const layout = React.useMemo(() => {
    const rs = range.start.getTime();
    const re = range.end.getTime();
    const out = new Map<
      string,
      {
        lanes: number;
        items: Array<{ b: Reservation; s: number; e: number; lane: number; left: number; right: number }>;
      }
    >();
    for (const room of rooms) {
      const items = normalized
        .filter((x) => x.b.roomId === room.id && x.e > rs && x.s < re && x.e > x.s)
        .sort((a, c) => a.s - c.s);
      const laneEnds: number[] = [];
      const placed = items.map((x) => {
        let lane = laneEnds.findIndex((end) => end <= x.s);
        if (lane === -1) {
          lane = laneEnds.length;
          laneEnds.push(x.e);
        } else {
          laneEnds[lane] = x.e;
        }
        return { ...x, lane, left: colPos(cols, x.s), right: colPos(cols, x.e) };
      });
      out.set(room.id.toString(), { lanes: Math.max(1, laneEnds.length), items: placed });
    }
    return out;
  }, [rooms, normalized, range, cols]);

  const fmt = React.useMemo(
    () => ({
      title:
        view === 'day'
          ? new Intl.DateTimeFormat(locale, { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })
          : view === 'month'
            ? new Intl.DateTimeFormat(locale, { year: 'numeric', month: 'long' })
            : new Intl.DateTimeFormat(locale, { month: 'short', day: 'numeric' }),
      weekday: new Intl.DateTimeFormat(locale, { weekday: view === 'month' ? 'narrow' : 'short' }),
      full: new Intl.DateTimeFormat(locale, { dateStyle: 'medium', timeStyle: view === 'day' ? 'short' : undefined }),
    }),
    [locale, view],
  );

  const title =
    view === 'week'
      ? `${fmt.title.format(range.start)} – ${fmt.title.format(addDays(range.end, -1))}, ${addDays(range.end, -1).getFullYear()}`
      : fmt.title.format(range.start);

  const shift = (dir: -1 | 1) => {
    if (view === 'day') setAnchor(addDays(anchor, dir));
    else if (view === 'week') setAnchor(addDays(anchor, 7 * dir));
    else setAnchor(new Date(anchor.getFullYear(), anchor.getMonth() + dir, 1));
  };

  const now = Date.now();
  const minColW = view === 'day' ? 34 : view === 'week' ? 80 : 30;
  const minW = `calc(${ROOM_W}px + ${cols.length * minColW}px)`;

  const isWeekend = (c: Column) => view !== 'day' && (c.date.getDay() === 0 || c.date.getDay() === 6);
  const isToday = (c: Column) => now >= c.start && now < c.end;

  const roomCellSx = {
    flex: `0 0 ${ROOM_W}px`,
    position: 'sticky',
    left: 0,
    zIndex: 3,
    bgcolor: 'background.paper',
    borderRight: 1,
    borderColor: 'divider',
    px: 1.25,
    display: 'flex',
    flexDirection: 'column',
    justifyContent: 'center',
    minWidth: 0,
  } as const;

  const columnBgSx = (c: Column) => [
    isWeekend(c) && { bgcolor: weekendBg },
    isToday(c) && { bgcolor: todayBg, fontWeight: 600 },
  ];

  const buildUpdateReq = (r: Reservation) => {
    const updateReq: UpdateReservation = {
      title: r.title,
      startTime: r.startTime,
      endTime: r.endTime,
      attendeeCount: r.attendeeCount,
      status: r.status
    } 
    return updateReq
  }

  return (
    <Paper variant="outlined" className={className} style={style} sx={{ borderRadius: 2, overflow: 'hidden' }}>
      {/* -------- Toolbar -------- */}
      <Box
        sx={{
          display: 'flex',
          flexWrap: 'wrap',
          gap: 1,
          alignItems: 'center',
          justifyContent: 'space-between',
          px: 1.5,
          py: 1,
          borderBottom: 1,
          borderColor: 'divider',
        }}
      >
        <Box sx={{ display: 'flex', gap: 0.5, alignItems: 'center' }}>
          <IconButton size="small" onClick={() => shift(-1)} aria-label="Previous">
            <ChevronLeftIcon fontSize="small" />
          </IconButton>
          <Button size="small" variant="outlined" onClick={() => setAnchor(startOfDay(new Date()))}>
            Today
          </Button>
          <IconButton size="small" onClick={() => shift(1)} aria-label="Next">
            <ChevronRightIcon fontSize="small" />
          </IconButton>
          <Typography sx={{ ml: 1, fontWeight: 600, textTransform: 'capitalize' }}>{title}</Typography>
        </Box>

        <ToggleButtonGroup
          size="small"
          exclusive
          value={view}
          onChange={(_, v: TimelineView | null) => v && setView(v)}
          aria-label="Zoom"
        >
          <ToggleButton value="day">Day</ToggleButton>
          <ToggleButton value="week">Week</ToggleButton>
          <ToggleButton value="month">Month</ToggleButton>
        </ToggleButtonGroup>
      </Box>

      {/* -------- Scrollable grid -------- */}
      <Box sx={{ overflowX: 'auto' }}>
        <Box sx={{ minWidth: minW }}>
          {/* Header row */}
          <Box sx={{ display: 'flex', borderBottom: 1, borderColor: 'divider' }}>
            <Box sx={{ ...roomCellSx, py: 0.5 }}>
              <Typography variant="caption" color="text.secondary">
                Rooms
              </Typography>
            </Box>
            <Box sx={{ flex: 1, display: 'flex', minWidth: 0 }}>
              {cols.map((c) => (
                <Box
                  key={c.start}
                  sx={[
                    {
                      flex: '1 1 0',
                      minWidth: 0,
                      textAlign: 'center',
                      py: 0.5,
                      borderLeft: 1,
                      borderColor: 'divider',
                      fontSize: 12,
                      lineHeight: 1.2,
                      '&:first-of-type': { borderLeft: 0 },
                    },
                    ...columnBgSx(c),
                  ]}
                >
                  {view === 'day' ? (
                    String(c.date.getHours()).padStart(2, '0')
                  ) : (
                    <>
                      <Typography
                        variant="caption"
                        component="span"
                        sx={{ display: 'block', fontSize: 11, color: 'text.secondary' }}
                      >
                        {fmt.weekday.format(c.date)}
                      </Typography>
                      {c.date.getDate()}
                    </>
                  )}
                </Box>
              ))}
            </Box>
          </Box>

          {rooms.length === 0 && (
            <Typography sx={{ p: 3, textAlign: 'center', color: 'text.secondary' }}>No rooms</Typography>
          )}

          {rooms.map((room) => {
            const { lanes, items } = layout.get(room.id.toString())!;
            const height = lanes * LANE_H + 8;

            return (
              <Box
                key={room.id}
                sx={{
                  display: 'flex',
                  borderBottom: 1,
                  borderColor: 'divider',
                  height,
                  '&:last-of-type': { borderBottom: 0 },
                }}
              >
                <Box sx={{ ...roomCellSx, py: 0.75 }}>
                  <Typography
                    noWrap
                    title={room.name}
                    sx={{ fontWeight: 600, fontSize: 14, lineHeight: 1.4 }}
                  >
                    {room.name}
                  </Typography>
                  
                </Box>

                <Box sx={{ position: 'relative', flex: '1 1 0', minWidth: 0 }}>
                  <Box sx={{ position: 'absolute', inset: 0, display: 'flex' }}>
                    {cols.map((c) => (
                      <Box
                        key={c.start}
                        onClick={() =>
                          onSlotClick?.({ roomId: room.id, start: new Date(c.start)})
                        }
                        sx={[
                          {
                            flex: '1 1 0',
                            minWidth: 0,
                            borderLeft: 1,
                            borderColor: 'divider',
                            cursor: 'pointer',
                            '&:first-of-type': { borderLeft: 0 },
                          },
                          ...columnBgSx(c),
                          { '&:hover': { bgcolor: hoverBg } },
                        ]}
                      />
                    ))}
                  </Box>

                  {items.filter(i => !showOnlyMy || i.b.userId === props.userId).map((it) => {
                    
                    const n = cols.length;
                    const width = Math.max(it.right - it.left, 0.3);
                    const clippedLeft = it.s < range.start.getTime();
                    const clippedRight = it.e > range.end.getTime();
                    const alpha = it.b.status === ReservationStatus.Booked ? 0.9 : 0.5
                    const color = it.b.userId === props.userId ? `rgba(169, 8, 255, ${alpha})` : `rgba(247, 155, 89, ${alpha})`
                    return (
                      
                      <Tooltip
                        key={it.b.id}
                        title={
                          <Box>
                            <Box sx={{ fontWeight: 400 }}>{ reservationStatusLabels[it.b.status] }</Box>
                            <Box sx={{ fontWeight: 600 }}>{it.b.userName} {it.b.userSurname}</Box>
                            <Box sx={{ fontSize: 12 }}>
                              {fmt.full.format(it.s)} → {fmt.full.format(it.e)}
                            </Box>
                            {it.b.title && <Box sx={{ fontSize: 12, opacity: 0.8 }}>{it.b.title}</Box>}
                          </Box>
                        }
                      >
                        <ButtonBase
                          disableRipple
                          onClick={() => onBookingClick?.(buildUpdateReq(it.b), it.b.id, it.b.userId)}
                          sx={{
                            position: 'absolute',
                            zIndex: 2,
                            height: BOOKING_H,
                            px: 1,
                            justifyContent: 'flex-start',
                            textAlign: 'left',
                            color: '#fff',
                            fontSize: 12,
                            fontWeight: 600,
                            borderRadius: 1,
                            boxShadow: 1,
                            bgcolor: color,
                            '&:hover': { bgcolor: color, filter: 'brightness(1.1)' },
                            '&:focus-visible': {
                              outline: 2,
                              outlineStyle: 'solid',
                              outlineColor: 'primary.main',
                              outlineOffset: 1,
                            },
                            left: `calc(${(it.left / n) * 100}% + 1px)`,
                            width: `calc(${(width / n) * 100}% - 2px)`,
                            top: 4 + it.lane * LANE_H + 4,
                            ...(clippedLeft && {
                              borderTopLeftRadius: 0,
                              borderBottomLeftRadius: 0,
                            }),
                            ...(clippedRight && {
                              borderTopRightRadius: 0,
                              borderBottomRightRadius: 0,
                            }),
                          }}
                        >
                          <Box
                            component="span"
                            sx={{ overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}
                          >
                            {it.b.userName} {it.b.userSurname}
                          </Box>
                        </ButtonBase>
                      </Tooltip>
                    );
                  })}
                </Box>
              </Box>
            );
          })}
        </Box>
      </Box>
    </Paper>
  );
}

export default RoomTimeline;